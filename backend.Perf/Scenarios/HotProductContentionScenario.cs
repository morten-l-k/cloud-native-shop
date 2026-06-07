using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

/// <summary>
/// This scenario tests database contention by simulating a "hot product" traffic pattern.
/// Instead of a uniform distribution, most traffic is funneled into a few specific database rows
/// (the stock of popular items), which realistically tests row-level locking and transaction overhead.
/// </summary>
public static class HotProductContentionScenario
{
    public static ScenarioProps Create(HttpClient http)
    {
        // Shared state between iterations
        var hotProductIds = new List<string>();
        var coldProductIds = new List<string>();
        var customerTokens = new List<string>();

        return Scenario.Create("hot_product_contention", async context =>
        {
            if (customerTokens.Count == 0 || hotProductIds.Count == 0)
                return Response.Fail(message: "Initialization failed or lists are empty.");

            // 1. Skew Logic (Zipfian-like/80-20 rule)
            var isHotRequest = Random.Shared.Next(100) < 80;
            var productId = isHotRequest 
                ? hotProductIds[Random.Shared.Next(hotProductIds.Count)]
                : coldProductIds[Random.Shared.Next(coldProductIds.Count)];

            var token = customerTokens[Random.Shared.Next(customerTokens.Count)];

            // --- STEP 1: View Product (GET) ---
            var step1 = await Step.Run("view_product", context, async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/Product/{productId}");
                var response = await http.SendAsync(request);
                return response.IsSuccessStatusCode 
                    ? Response.Ok() 
                    : Response.Fail(statusCode: ((int)response.StatusCode).ToString());
            });

            // --- STEP 2: Think Time ---
            await Task.Delay(Random.Shared.Next(500, 1000));

            // --- STEP 3: Checkout / Deduct Inventory (POST) ---
            var step3 = await Step.Run("checkout_contention", context, async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/order");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(new
                {
                    items = new[]
                    {
                        new { productId, quantity = 1, price = 10.00m }
                    }
                });

                var response = await http.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Created) return Response.Ok(statusCode: "201");
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict) return Response.Ok(statusCode: "409");
                
                return Response.Fail(statusCode: ((int)response.StatusCode).ToString());
            });

            return Response.Ok();
        })
        .WithInit(async context =>
        {
            context.Logger.Information("Initializing Hot Product Contention Scenario...");

            // 1. Fetch available products from the database
            var productRes = await http.GetAsync("/Product?page=1&pageSize=50");
            productRes.EnsureSuccessStatusCode();
            var body = await productRes.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            var allIds = doc.RootElement.GetProperty("items")
                .EnumerateArray()
                .Select(p => p.GetProperty("id").GetString()!)
                .ToList();

            if (allIds.Count < 10)
                throw new InvalidOperationException("Not enough products in DB to run contention test.");

            // Designate "Hot" vs "Cold" products
            hotProductIds.AddRange(allIds.Take(3)); // Top 3 are "popular"
            coldProductIds.AddRange(allIds.Skip(3));

            context.Logger.Information($"Skew Config: 80% traffic targeting {hotProductIds.Count} items; 20% targeting {coldProductIds.Count} items.");

            // 2. Pre-register a pool of customers to avoid bottlenecking on the Auth provider
            context.Logger.Information("Pre-registering 100 customers...");
            for (int i = 0; i < 100; i++)
            {
                var email = $"contention-user-{i}-{Guid.NewGuid():N}@test.com";
                var regRes = await http.PostAsJsonAsync("/auth/register/customer", new
                {
                    password = "password",
                    customerZipCodePrefix = "12345",
                    customerCity = "PerfCity",
                    customerState = "PC",
                    firstName = "Perf",
                    lastName = $"User{i}",
                    emailAddress = email,
                    streetAddress = "Load Street"
                });

                if (regRes.IsSuccessStatusCode)
                {
                    var regBody = await regRes.Content.ReadAsStringAsync();
                    var token = JsonDocument.Parse(regBody).RootElement.GetProperty("token").GetString();
                    if (token != null) customerTokens.Add(token);
                }
            }
            context.Logger.Information($"Successfully pre-registered {customerTokens.Count} customers.");
        })
        .WithLoadSimulations(
            // Ramp up to 100 iterations/sec over 1 minute.
            // This allows us to observe the latency degradation as concurrency increases.
            Simulation.RampingInject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        )
        // Set a small warm-up to initialize NBomber's internal timers
        .WithWarmUpDuration(TimeSpan.FromSeconds(1));
    }
}

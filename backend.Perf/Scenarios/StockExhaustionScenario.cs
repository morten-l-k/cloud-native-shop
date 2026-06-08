using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class StockExhaustionScenario
{
    public static ScenarioProps Create(HttpClient http)
    {
        string? perfTestProductId = null;
        const int initialStock = 1000; 
        const string sellerId = "3442f8959a84dea7ee197c632cb2df15"; // Sample seller from dataset
        var tokens = new List<string>();

        return Scenario.Create("stock_exhaustion_concurrency", async context =>
        {
            if (tokens.Count == 0)
                return Response.Fail(message: "No tokens available. Initialization might have failed.");

            // Step 1: Pick a random pre-registered customer
            var token = tokens[Random.Shared.Next(tokens.Count)];

            // Step 2: Attempt to buy the limited stock product
            // We use a shared HttpClient and HttpRequestMessage to minimize overhead
            using var request = new HttpRequestMessage(HttpMethod.Post, "/order");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new
            {
                items = new[]
                {
                    new { productId = perfTestProductId, quantity = 1, price = 10.00m }
                }
            });

            var response = await http.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return Response.Ok(statusCode: "201");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // We mark 409 as Fail to show it clearly in the report as the point where stock is gone.
                // This creates a visible "cliff" in the OK vs Fail graph.
                return Response.Fail(message: "Stock Exhausted", statusCode: "409");
            }

            return Response.Fail(message: $"Unexpected Status: {response.StatusCode}", statusCode: ((int)response.StatusCode).ToString());
        })
        .WithInit(async context =>
        {
            context.Logger.Information("Initializing Stock Exhaustion Scenario...");
            
            // 1. Log in as seller to create the product
            var loginRes = await http.PostAsJsonAsync("/auth/login/seller", new { id = sellerId, password = "password" });
            loginRes.EnsureSuccessStatusCode();
            var loginBody = await loginRes.Content.ReadAsStringAsync();
            var sellerToken = JsonDocument.Parse(loginBody).RootElement.GetProperty("token").GetString();

            using var sellerClient = new HttpClient { BaseAddress = http.BaseAddress };
            sellerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sellerToken);

            // 2. Create the limited stock product
            var createRes = await sellerClient.PostAsJsonAsync("/product", new
            {
                name = "Limited Edition Perf Product",
                category = "eletronicos",
                description = "High concurrency test product",
                price = 10.00m,
                stock = initialStock
            });
            createRes.EnsureSuccessStatusCode();
            var createBody = await createRes.Content.ReadAsStringAsync();
            perfTestProductId = JsonDocument.Parse(createBody).RootElement.GetProperty("productId").GetString();
            
            context.Logger.Information($"Created product {perfTestProductId} with {initialStock} units.");

            // 3. Pre-register a pool of customers to avoid registration overhead during the test
            context.Logger.Information("Pre-registering 100 customers...");
            for (int i = 0; i < 100; i++)
            {
                var email = $"stock-perf-{i}-{Guid.NewGuid():N}@test.com";
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
                    if (token != null) tokens.Add(token);
                }
            }
            context.Logger.Information($"Successfully pre-registered {tokens.Count} customers.");
        })
        .WithLoadSimulations(
            // Ramp up from 1 to 100 requests per second over 30 seconds.
            Simulation.RampingInject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        )
        .WithWarmUpDuration(TimeSpan.FromSeconds(1));
    }
}


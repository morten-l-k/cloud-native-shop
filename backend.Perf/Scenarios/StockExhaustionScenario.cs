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
        const int initialStock = 100;
        const string sellerId = "3442f8959a84dea7ee197c632cb2df15"; // Sample seller from dataset

        return Scenario.Create("stock_exhaustion_concurrency", async context =>
        {
            // Step 1: Create a unique customer for this iteration to avoid login conflicts
            var uniqueEmail = $"perf-{Guid.NewGuid():N}@exhaustion.dev";
            var registerResponse = await http.PostAsJsonAsync("/auth/register/customer", new
            {
                password = "password",
                customerZipCodePrefix = "12345",
                customerCity = "PerfCity",
                customerState = "PC",
                firstName = "Race",
                lastName = "Condition",
                emailAddress = uniqueEmail,
                streetAddress = "Concurrency Lane 1"
            });

            if (!registerResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"Register failed: {registerResponse.StatusCode}");

            var registerBody = await registerResponse.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(registerBody).RootElement.GetProperty("token").GetString();

            using var authed = new HttpClient { BaseAddress = http.BaseAddress };
            authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Step 2: Attempt to buy the limited stock product
            // We use a stopwatch to measure the exact time the DB transaction takes
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var orderResponse = await authed.PostAsJsonAsync("/order", new
            {
                items = new[]
                {
                    new { productId = perfTestProductId, quantity = 1, price = 10.00m }
                }
            });

            watch.Stop();

            if (orderResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return Response.Ok(sizeBytes: 0, statusCode: "201");
            }
            
            if (orderResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // This is an "expected failure" in this scenario
                return Response.Ok(sizeBytes: 0, statusCode: "409");
            }

            return Response.Fail(message: $"Unexpected Status: {orderResponse.StatusCode}", statusCode: ((int)orderResponse.StatusCode).ToString());
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
        })
        .WithLoadSimulations(
            // Burst: Start with a heavy burst to force many concurrent requests hitting the same row
            Simulation.RampingInject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        )
        .WithWarmUpDuration(TimeSpan.FromSeconds(5));
    }
}

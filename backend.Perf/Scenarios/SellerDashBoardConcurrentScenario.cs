using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class SellerDashboardConcurrentScenario
{
    public static ScenarioProps Create(HttpClient http, int productCount, int sellerCount)
    {
        var sellerClients = new List<HttpClient>();

        return Scenario.Create($"seller_dashboard_{productCount}_products_{sellerCount}_sellers", async context =>
        {
            if (sellerClients.Count == 0)
                return Response.Fail(message: "No seller clients were initialized.");

            var sellerClient = sellerClients[Random.Shared.Next(sellerClients.Count)];

            using var response = await sellerClient.GetAsync("/Product/seller");

            if (!response.IsSuccessStatusCode)
                return Response.Fail(message: $"Seller dashboard failed: HTTP {(int)response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync();

            return Response.Ok(sizeBytes: body.Length);
        })
        .WithInit(async context =>
        {
            context.Logger.Information(
                $"Seeding {sellerCount} sellers with {productCount} products each..."
            );

            for (var sellerIndex = 0; sellerIndex < sellerCount; sellerIndex++)
            {
                var sellerId = $"perf-seller-{sellerIndex}-{Guid.NewGuid():N}";

                var register = await http.PostAsJsonAsync("/auth/register/seller", new
                {
                    id = sellerId,
                    password = "password",
                    sellerZipCodePrefix = "12345",
                    sellerCity = "PerfCity",
                    sellerState = "PC"
                });

                register.EnsureSuccessStatusCode();

                var body = await register.Content.ReadAsStringAsync();
                var token = JsonDocument.Parse(body).RootElement.GetProperty("token").GetString();

                var sellerClient = new HttpClient { BaseAddress = http.BaseAddress };
                sellerClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                sellerClients.Add(sellerClient);

                for (var productIndex = 0; productIndex < productCount; productIndex++)
                {
                    var create = await sellerClient.PostAsJsonAsync("/Product", new
                    {
                        name = $"Perf Product {sellerIndex}-{productIndex}",
                        category = "eletronicos",
                        description = "Concurrent seller dashboard test product",
                        price = 10.00m,
                        stock = 100
                    });

                    create.EnsureSuccessStatusCode();
                }

                context.Logger.Information(
                    $"Seeded seller {sellerIndex + 1}/{sellerCount}"
                );
            }

            context.Logger.Information(
                $"Finished seeding {sellerCount} sellers and {sellerCount * productCount} products."
            );
        })
        .WithClean(context =>
        {
            foreach (var client in sellerClients)
                client.Dispose();

            sellerClients.Clear();

            return Task.CompletedTask;
        })
        .WithLoadSimulations(
            Simulation.Inject(
                rate: sellerCount,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30)
            )
        )
        .WithWarmUpDuration(TimeSpan.FromSeconds(5));
    }
}
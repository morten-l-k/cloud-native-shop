using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class SellerDashboardScalingScenario
{
    public static ScenarioProps Create(HttpClient http, int productCount)
    {
        HttpClient? sellerClient = null;

        return Scenario.Create($"seller_dashboard_{productCount}_products", async context =>
        {
            using var response = await sellerClient!.GetAsync("/Product/seller");

            if (!response.IsSuccessStatusCode)
                return Response.Fail(message: $"Seller dashboard failed: HTTP {(int)response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync();

            return Response.Ok(sizeBytes: body.Length);
        })
        .WithInit(async context =>
        {
            var sellerId = $"perf-seller-{Guid.NewGuid():N}";

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

            sellerClient = new HttpClient { BaseAddress = http.BaseAddress };
            sellerClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            for (var i = 0; i < productCount; i++)
            {
                var create = await sellerClient.PostAsJsonAsync("/Product", new
                {
                    name = $"Perf Product {i}",
                    category = "eletronicos",
                    description = "Seller dashboard scaling test product",
                    price = 10.00m,
                    stock = 100
                });

                create.EnsureSuccessStatusCode();
            }

            context.Logger.Information($"Seeded {productCount} products for seller {sellerId}");
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        )
        .WithWarmUpDuration(TimeSpan.FromSeconds(5));
    }
}
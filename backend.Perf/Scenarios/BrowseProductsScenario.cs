using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class BrowseProductsScenario
{
    public static ScenarioProps Create(HttpClient http)
    {
        return Scenario.Create("browse_products", async context =>
        {
            // Step 1: fetch a random page of product listings
            var page = Random.Shared.Next(1, 6);
            using var listResponse = await http.GetAsync($"/Product?page={page}");

            if (!listResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"List failed: HTTP {(int)listResponse.StatusCode}");

            var body = await listResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var items = doc.RootElement.GetProperty("items");

            if (items.GetArrayLength() == 0)
                return Response.Ok();

            // Step 2: fetch the detail page for a randomly chosen product
            var idx = Random.Shared.Next(0, items.GetArrayLength());
            var productId = items[idx].GetProperty("id").GetString();
            if (productId is null)
                return Response.Fail(message: "Product id was null");

            using var detailResponse = await http.GetAsync($"/Product/{productId}");

            if (!detailResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"Detail failed: HTTP {(int)detailResponse.StatusCode}");

            return Response.Ok();
        })
        .WithLoadSimulations(
            // Ramp up from 0 to 10 req/s over the first 30 seconds
            Simulation.RampingInject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            // Hold at 10 req/s for 60 seconds to get stable measurements
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );
    }
}

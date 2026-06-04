using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class CheckoutScenario
{
    public static ScenarioProps Create(HttpClient http)
    {
        string[] emails = [];
        string[] productIds = [];

        return Scenario.Create("checkout", async context =>
        {
            // Step 1: Login as an existing customer
            var email = emails[Random.Shared.Next(0, emails.Length)];
            using var loginRes = await http.PostAsJsonAsync("/auth/login/customer", new { email, password = "password" });
            if (!loginRes.IsSuccessStatusCode)
                return Response.Fail(statusCode: $"login_{(int)loginRes.StatusCode}", message: $"login HTTP {(int)loginRes.StatusCode}");

            using var loginDoc = JsonDocument.Parse(await loginRes.Content.ReadAsStringAsync());
            var token = loginDoc.RootElement.GetProperty("token").GetString();
            if (string.IsNullOrEmpty(token))
                return Response.Fail(statusCode: "login_token_null", message: "login:token null");

            using var authed = new HttpClient { BaseAddress = http.BaseAddress };
            authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Step 2: Pick a random product from the seeded list
            var productId = productIds[Random.Shared.Next(0, productIds.Length)];

            // Step 3: Place the order
            using var orderRes = await authed.PostAsJsonAsync("/order", new
            {
                items = new[] { new { productId, quantity = 1, price = 49.99m } }
            });

            if (!orderRes.IsSuccessStatusCode)
                return Response.Fail(statusCode: "order_failed", message: $"place_order HTTP {(int)orderRes.StatusCode}");

            using var orderDoc = JsonDocument.Parse(await orderRes.Content.ReadAsStringAsync());
            var orderId = orderDoc.RootElement.GetProperty("orderId").GetString();
            if (string.IsNullOrEmpty(orderId))
                return Response.Fail(statusCode: "order_id_null", message: "place_order:orderId null");

            // Step 4: Pay for the order (2.5% of requests are intentionally declined by the server)
            using var payRes = await authed.PostAsJsonAsync("/payment", new { orderId });

            if (payRes.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
                return Response.Ok(statusCode: "402_declined");

            return payRes.IsSuccessStatusCode
                ? Response.Ok(statusCode: "200_paid")
                : Response.Fail(statusCode: $"payment_{(int)payRes.StatusCode}", message: $"payment HTTP {(int)payRes.StatusCode}");
        })
        .WithInit(async context =>
        {
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "database", "data", "olist_customers_dataset.csv");
            emails = [.. File.ReadAllLines(csvPath)
                .Skip(1)
                .Select(line => line.Split(',')[7])
                .Where(e => !string.IsNullOrWhiteSpace(e))];

            using var res = await http.GetAsync("/Product?page=1&pageSize=50");
            res.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            productIds = [.. doc.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Select(p => p.GetProperty("id").GetString()!)
                .Where(id => !string.IsNullOrEmpty(id))];

            if (productIds.Length == 0)
                throw new InvalidOperationException("No products found — seed the database before running.");

            context.Logger.Information("Checkout scenario ready — {Emails} emails, {Count} products available", emails.Length, productIds.Length);
        })
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }
}

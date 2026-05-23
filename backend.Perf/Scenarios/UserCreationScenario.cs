using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class UserCreationScenario
{
    public static ScenarioProps Create(HttpClient http)
    {
        string[] productIds = [];

        return Scenario.Create("user_creation_order_dashboard", async context =>
        {
            // Step 1: Register a new customer
            var uniqueEmail = $"perf-user-{Guid.NewGuid():N}@loadtest.dev";
            using var registerResponse = await http.PostAsJsonAsync("/auth/register/customer", new
            {
                password = "password",
                customerZipCodePrefix = "10001",
                customerCity = "New York",
                customerState = "NY",
                firstName = "Perf",
                lastName = "User",
                emailAddress = uniqueEmail,
                streetAddress = "123 Load Test Ave"
            });

            if (!registerResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"Register failed: HTTP {(int)registerResponse.StatusCode}");

            var registerBody = await registerResponse.Content.ReadAsStringAsync();
            using var registerDoc = JsonDocument.Parse(registerBody);
            var token = registerDoc.RootElement.GetProperty("token").GetString();

            if (string.IsNullOrEmpty(token))
                return Response.Fail(message: "Token missing after registration");

            // Step 2: Log in with the newly created account
            using var loginResponse = await http.PostAsJsonAsync("/auth/login/customer", new
            {
                email = uniqueEmail,
                password = "password"
            });

            if (!loginResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"Login failed: HTTP {(int)loginResponse.StatusCode}");

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginBody);
            token = loginDoc.RootElement.GetProperty("token").GetString();

            if (string.IsNullOrEmpty(token))
                return Response.Fail(message: "Token missing after login");

            using var authed = new HttpClient { BaseAddress = http.BaseAddress };
            authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Step 3: Place an order
            var productId = productIds[Random.Shared.Next(0, productIds.Length)];
            using var orderResponse = await authed.PostAsJsonAsync("/order", new
            {
                items = new[]
                {
                    new { productId, quantity = 1, price = 49.99m }
                }
            });

            if (!orderResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"PlaceOrder failed: HTTP {(int)orderResponse.StatusCode}");

            // Step 4: Fetch order history (dashboard)
            using var dashboardResponse = await authed.GetAsync("/order/me");

            if (!dashboardResponse.IsSuccessStatusCode)
                return Response.Fail(message: $"Dashboard failed: HTTP {(int)dashboardResponse.StatusCode}");

            return Response.Ok();
        })
        .WithInit(async context =>
        {
            // Fetch the first page of products so we have real product IDs to order
            using var res = await http.GetAsync("/Product?page=1&pageSize=50");
            res.EnsureSuccessStatusCode();

            var body = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            productIds = [.. doc.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Select(p => p.GetProperty("id").GetString()!)
                .Where(id => !string.IsNullOrEmpty(id))];

            if (productIds.Length == 0)
                throw new InvalidOperationException("No products found — cannot run order scenario.");
        })
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );
    }
}

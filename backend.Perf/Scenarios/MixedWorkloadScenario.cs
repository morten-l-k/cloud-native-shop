using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class MixedWorkloadScenario
{
    private const int WeightCheckout = 30;
    private const int WeightBrowseProducts = 30;
    private const int WeightCustomerLogin = 15;
    private const int WeightSellerDashboard = 15;
    private const int WeightUserCreationOrder = 5;
    private const int WeightStockExhaustion = 5;

    // Weights must sum to 100 for readable percentages, but any positive integers work.
    private static readonly (string Name, int Weight)[] Weights =
    [
        ("checkout",            WeightCheckout),
        ("browse_products",     WeightBrowseProducts),
        ("customer_login",      WeightCustomerLogin),
        ("seller_dashboard",    WeightSellerDashboard),
        ("user_creation_order", WeightUserCreationOrder),
        ("stock_exhaustion",    WeightStockExhaustion),
    ];

    private static readonly int TotalWeight = Weights.Sum(t => t.Weight);

    public static ScenarioProps Create(HttpClient http)
    {
        string[] emails = [];
        string[] productIds = [];
        HttpClient? sellerClient = null;
        string? stockProductId = null;

        return Scenario.Create("mixed_workload", async context =>
        {
            var transaction = Pick();

            return transaction switch
            {
                "checkout" => await Checkout(http, emails, productIds),
                "browse_products" => await BrowseProducts(http),
                "customer_login" => await CustomerLogin(http, emails),
                "seller_dashboard" => await SellerDashboard(sellerClient!),
                "user_creation_order" => await UserCreationOrder(http, productIds),
                "stock_exhaustion" => await StockExhaustion(http, stockProductId!),
                _ => Response.Fail(message: $"Unknown transaction: {transaction}")
            };
        })
        .WithInit(async context =>
        {
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "database", "data", "olist_customers_dataset.csv");
            emails = File.ReadAllLines(csvPath)
                .Skip(1)
                .Select(line => line.Split(',')[7])
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();

            using var productRes = await http.GetAsync("/Product?page=1&pageSize=50");
            productRes.EnsureSuccessStatusCode();
            using var productDoc = JsonDocument.Parse(await productRes.Content.ReadAsStringAsync());
            productIds = productDoc.RootElement.GetProperty("items").EnumerateArray()
                .Select(p => p.GetProperty("id").GetString()!)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToArray();

            if (productIds.Length == 0)
                throw new InvalidOperationException("No products found — seed the database before running.");

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

            using var registerDoc = JsonDocument.Parse(await register.Content.ReadAsStringAsync());
            var sellerToken = registerDoc.RootElement.GetProperty("token").GetString();

            sellerClient = new HttpClient { BaseAddress = http.BaseAddress };
            sellerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sellerToken);

            for (var i = 0; i < 20; i++)
            {
                var p = await sellerClient.PostAsJsonAsync("/Product", new
                {
                    name = $"Mixed Workload Product {i}",
                    category = "eletronicos",
                    description = "Mixed workload test product",
                    price = 29.99m,
                    stock = 10_000
                });
                p.EnsureSuccessStatusCode();
            }

            var stockRes = await sellerClient.PostAsJsonAsync("/product", new
            {
                name = "Mixed Workload Limited Product",
                category = "eletronicos",
                description = "Stock concurrency test product",
                price = 10.00m,
                stock = 10_000
            });
            stockRes.EnsureSuccessStatusCode();

            using var stockDoc = JsonDocument.Parse(await stockRes.Content.ReadAsStringAsync());
            stockProductId = stockDoc.RootElement.GetProperty("productId").GetString();

            context.Logger.Information(
                "Mixed workload ready — emails: {Emails}, products: {Products}, stockProduct: {StockId}",
                emails.Length, productIds.Length, stockProductId);
        })
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    // Weighted random selection: rolls in [0, TotalWeight) and walks the cumulative sum.
    private static string Pick()
    {
        var roll = Random.Shared.Next(0, TotalWeight);
        var cumulative = 0;
        foreach (var (name, weight) in Weights)
        {
            cumulative += weight;
            if (roll < cumulative)
                return name;
        }
        return Weights[^1].Name;
    }

    private static async Task<IResponse> Checkout(HttpClient http, string[] emails, string[] productIds)
    {
        // Step 1: Login as an existing customer
        var email = emails[Random.Shared.Next(0, emails.Length)];
        using var loginRes = await http.PostAsJsonAsync("/auth/login/customer", new { email, password = "password" });
        if (!loginRes.IsSuccessStatusCode)
            return Response.Fail(statusCode: "login_failed", message: $"login HTTP {(int)loginRes.StatusCode}");

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
            return Response.Fail(message: $"place_order HTTP {(int)orderRes.StatusCode}");

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
            : Response.Fail(statusCode: "payment_failed", message: $"payment HTTP {(int)payRes.StatusCode}");
    }

    private static async Task<IResponse> BrowseProducts(HttpClient http)
    {
        var page = Random.Shared.Next(1, 6);
        using var listRes = await http.GetAsync($"/Product?page={page}");
        if (!listRes.IsSuccessStatusCode)
            return Response.Fail(message: $"browse:list HTTP {(int)listRes.StatusCode}");

        using var doc = JsonDocument.Parse(await listRes.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        if (items.GetArrayLength() == 0)
            return Response.Ok(statusCode: "browse_products");

        var productId = items[Random.Shared.Next(0, items.GetArrayLength())].GetProperty("id").GetString();
        if (productId is null)
            return Response.Fail(message: "browse:product id null");

        using var detailRes = await http.GetAsync($"/Product/{productId}");
        return detailRes.IsSuccessStatusCode
            ? Response.Ok(statusCode: "browse_products")
            : Response.Fail(message: $"browse:detail HTTP {(int)detailRes.StatusCode}");
    }

    private static async Task<IResponse> CustomerLogin(HttpClient http, string[] emails)
    {
        var email = emails[Random.Shared.Next(0, emails.Length)];
        using var res = await http.PostAsJsonAsync("/auth/login/customer", new { email, password = "password" });
        if (!res.IsSuccessStatusCode)
            return Response.Fail(message: $"login HTTP {(int)res.StatusCode}");

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("token").GetString();
        return string.IsNullOrEmpty(token)
            ? Response.Fail(message: "login:token null")
            : Response.Ok(statusCode: "customer_login");
    }

    private static async Task<IResponse> SellerDashboard(HttpClient sellerClient)
    {
        using var res = await sellerClient.GetAsync("/Product/seller");
        if (!res.IsSuccessStatusCode)
            return Response.Fail(message: $"seller_dashboard HTTP {(int)res.StatusCode}");

        var body = await res.Content.ReadAsStringAsync();
        return Response.Ok(sizeBytes: body.Length, statusCode: "seller_dashboard");
    }

    private static async Task<IResponse> UserCreationOrder(HttpClient http, string[] productIds)
    {
        var email = $"perf-{Guid.NewGuid():N}@loadtest.dev";
        using var regRes = await http.PostAsJsonAsync("/auth/register/customer", new
        {
            password = "password",
            customerZipCodePrefix = "10001",
            customerCity = "New York",
            customerState = "NY",
            firstName = "Perf",
            lastName = "User",
            emailAddress = email,
            streetAddress = "123 Load Test Ave"
        });
        if (!regRes.IsSuccessStatusCode)
            return Response.Fail(message: $"user_creation:register HTTP {(int)regRes.StatusCode}");

        using var regDoc = JsonDocument.Parse(await regRes.Content.ReadAsStringAsync());
        var token = regDoc.RootElement.GetProperty("token").GetString();

        using var authed = new HttpClient { BaseAddress = http.BaseAddress };
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var productId = productIds[Random.Shared.Next(0, productIds.Length)];
        using var orderRes = await authed.PostAsJsonAsync("/order", new
        {
            items = new[] { new { productId, quantity = 1, price = 49.99m } }
        });
        if (!orderRes.IsSuccessStatusCode)
            return Response.Fail(message: $"user_creation:order HTTP {(int)orderRes.StatusCode}");

        using var dashRes = await authed.GetAsync("/order/me");
        return dashRes.IsSuccessStatusCode
            ? Response.Ok(statusCode: "user_creation_order")
            : Response.Fail(message: $"user_creation:dashboard HTTP {(int)dashRes.StatusCode}");
    }

    private static async Task<IResponse> StockExhaustion(HttpClient http, string stockProductId)
    {
        var email = $"perf-{Guid.NewGuid():N}@mixed.dev";
        var regRes = await http.PostAsJsonAsync("/auth/register/customer", new
        {
            password = "password",
            customerZipCodePrefix = "12345",
            customerCity = "PerfCity",
            customerState = "PC",
            firstName = "Race",
            lastName = "Condition",
            emailAddress = email,
            streetAddress = "Concurrency Lane 1"
        });
        if (!regRes.IsSuccessStatusCode)
            return Response.Fail(message: $"stock:register HTTP {regRes.StatusCode}");

        using var regDoc = JsonDocument.Parse(await regRes.Content.ReadAsStringAsync());
        var token = regDoc.RootElement.GetProperty("token").GetString();

        using var authed = new HttpClient { BaseAddress = http.BaseAddress };
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var orderRes = await authed.PostAsJsonAsync("/order", new
        {
            items = new[] { new { productId = stockProductId, quantity = 1, price = 10.00m } }
        });

        return orderRes.StatusCode is System.Net.HttpStatusCode.Created or System.Net.HttpStatusCode.Conflict
            ? Response.Ok(statusCode: "stock_exhaustion")
            : Response.Fail(message: $"stock:order HTTP {orderRes.StatusCode}");
    }
}

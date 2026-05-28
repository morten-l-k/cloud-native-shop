using System.Net.Http.Json;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

namespace backend.Perf.Scenarios;

public static class CustomerLogInScenario
{
    public static ScenarioProps Create(HttpClient http, int userCount)
    {
        string[] emails = [];

        return Scenario.Create($"customer_log_in_{userCount}", async context =>
        {
            var email = emails[Random.Shared.Next(0, emails.Length)];

            using var response = await http.PostAsJsonAsync("/auth/login/customer", new { email, password = "password" });

            if (!response.IsSuccessStatusCode)
                return Response.Fail(message: $"Login failed: HTTP {(int)response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var token = doc.RootElement.GetProperty("token").GetString();

            if (string.IsNullOrEmpty(token))
                return Response.Fail(message: "Token was null or empty");

            return Response.Ok();
        })
        .WithInit(context =>
        {
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "database", "data", "olist_customers_dataset.csv");
            emails = File.ReadAllLines(csvPath)
                .Skip(1)
                .Select(line => line.Split(',')[7])
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();
            return Task.CompletedTask;
        })
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );
    }
}


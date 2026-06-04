using backend.Perf.Scenarios;
using NBomber.CSharp;

var baseUrl = args.ElementAtOrDefault(0) ?? "http://localhost:8080";
var targetFilter = args.ElementAtOrDefault(1);

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

var scenarios = new[]
{
    BrowseProductsScenario.Create(http),
    CustomerLogInScenario.Create(http, 10),
    CustomerLogInScenario.Create(http, 500),
    UserCreationScenario.Create(http),
    StockExhaustionScenario.Create(http),

    SellerDashboardScalingScenario.Create(http, 10),
    SellerDashboardScalingScenario.Create(http, 100),
    SellerDashboardScalingScenario.Create(http, 500),
    SellerDashboardScalingScenario.Create(http, 1000),
    
    CheckoutScenario.Create(http),

    MixedWorkloadScenario.Create(http),
}.ToArray();


var runner = NBomberRunner.RegisterScenarios(scenarios);

if (targetFilter is not null)
{
    var matched = scenarios
        .Select(s => s.ScenarioName)
        .Where(n => n.StartsWith(targetFilter, StringComparison.OrdinalIgnoreCase))
        .ToArray();
    runner = runner.WithTargetScenarios(matched);
}

runner.Run();
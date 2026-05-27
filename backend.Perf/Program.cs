using backend.Perf.Scenarios;
using NBomber.CSharp;

var baseUrl = args.ElementAtOrDefault(0) ?? "http://localhost:8080";
var targetScenario = args.ElementAtOrDefault(1);

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

var scenarios = new[]
{
    BrowseProductsScenario.Create(http),
    CustomerLogInScenario.Create(http),
    UserCreationScenario.Create(http),
    StockExhaustionScenario.Create(http),

    SellerDashboardScalingScenario.Create(http, 10),
    SellerDashboardScalingScenario.Create(http, 100),
    SellerDashboardScalingScenario.Create(http, 500),
    SellerDashboardScalingScenario.Create(http, 1000),
};

var runner = NBomberRunner.RegisterScenarios(scenarios);

if (targetScenario is not null)
    runner = runner.WithTargetScenarios(targetScenario);

runner.Run();
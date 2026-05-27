using backend.Perf.Scenarios;
using NBomber.CSharp;

var baseUrl = args.ElementAtOrDefault(0) ?? "http://localhost:8080";
var targetScenario = args.ElementAtOrDefault(1);

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

var runner = NBomberRunner
    .RegisterScenarios(
        BrowseProductsScenario.Create(http),
        CustomerLogInScenario.Create(http),
        UserCreationScenario.Create(http),
        StockExhaustionScenario.Create(http)
    );

if (targetScenario is not null)
    runner = runner.WithTargetScenarios(targetScenario);

runner.Run();

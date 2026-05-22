using backend.Perf.Scenarios;
using NBomber.CSharp;

var baseUrl = args.FirstOrDefault() ?? "http://localhost:8080";

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

NBomberRunner
    .RegisterScenarios(
        BrowseProductsScenario.Create(http),
        CustomerLogInScenario.Create(http)
    )
    .Run();

# Performance Testing with NBomber
This project uses NBomber to perform performance testing on the backend of the Cloud Native Shop application. NBomber allows us to simulate various scenarios and measure the performance of our API under load.

The testing is placed in the `backend.Perf` project, which contains the necessary code to define and run performance tests. The results of these tests are stored locally (only) in the `reports` folder within the `backend.Perf` directory.

# To run performance tests, use the following command in the terminal from the backend.Perf directory:
```bash
dotnet run
```

# To run only a specific scenario, pass a name prefix as the second argument:
```bash
dotnet run -- <baseUrl> <scenarioPrefix>
```

| Scenario | Command |
|---|---|
| Browse products | `dotnet run -- http://localhost:8080 browse_products` |
| Customer login (10 users) | `dotnet run -- http://localhost:8080 customer_log_in_10` |
| Customer login (500 users) | `dotnet run -- http://localhost:8080 customer_log_in_500` |
| User creation + order | `dotnet run -- http://localhost:8080 user_creation_order_dashboard` |
| Stock exhaustion | `dotnet run -- http://localhost:8080 stock_exhaustion_concurrency` |
| Checkout | `dotnet run -- http://localhost:8080 checkout` |
| Seller dashboard (10 products) | `dotnet run -- http://localhost:8080 seller_dashboard_10` |
| Seller dashboard (100 products) | `dotnet run -- http://localhost:8080 seller_dashboard_100` |
| Seller dashboard (500 products) | `dotnet run -- http://localhost:8080 seller_dashboard_500` |
| Seller dashboard (1000 products) | `dotnet run -- http://localhost:8080 seller_dashboard_1000` |
| All seller dashboard variants | `dotnet run -- http://localhost:8080 seller_dashboard` |
| Mixed workload | `dotnet run -- http://localhost:8080 mixed_workload` |
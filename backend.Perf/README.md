# Performance Testing with NBomber
This project uses NBomber to perform performance testing on the backend of the Cloud Native Shop application. NBomber allows us to simulate various scenarios and measure the performance of our API under load.

The testing is placed in the `backend.Perf` project, which contains the necessary code to define and run performance tests. The results of these tests are stored locally (only) in the `reports` folder within the `backend.Perf` directory.

# To run performance tests, use the following command in the terminal from the backend.Perf directory:
```bash
dotnet run
```
## Run backend application outside container
To run backend web application without using docker containers, then run the following from the ./backend folder:

```
dotnet run
```

Notice that when run outside docker containers, it will run on the ports specified in `./backend/Properties/launchSettings.json`

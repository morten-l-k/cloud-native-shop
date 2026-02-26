## For development of backend
To run backend web application without using docker containers, then run the following from the ./backend folder:

```
dotnet watch run
```

Notice that when run outside docker containers, it will run on the ports specified in `./backend/Properties/launchSettings.json`

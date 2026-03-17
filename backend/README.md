## For development of backend
To run backend web application without using docker containers, then run the following from the ./backend folder:

```
dotnet watch run
```

Notice that when run outside docker containers, it will run on the ports specified in `./backend/Properties/launchSettings.json`

# Software architecture
The backend is built using ASP.NET Core Web API. It follows a layered architecture with the following layers:
- **Models**: Represent the data structures used in the application. CRUD operations are performed on these models.
- **Controllers**: Handle HTTP requests from the client. They are responsible for receiving input from the client and returning appropriate responses.
- **Views**: Responsible for rendering the user interface.

## API Documentation
The backend API is documented using Swagger. When the application is running, you can access the Swagger UI at `http://localhost:8080/swagger` to explore the available API endpoints and their documentation
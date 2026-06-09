# B2C platform

## Docker setup
This project uses Docker Compose to run:
- PostgreSQL database (container name: b2c_postgres)
- ASP.NET Backend web application (container name: b2c_backend)
- Angular frontend (container name: b2c_frontend)

### Run docker environment in development mode
To run the docker environment in development mode, run the following command from the project root:
```bash
docker compose watch
```

### Start all containers
To start all containers, run the command from the project root:
```bash
docker compose up --build
```
### Access the database
After having run the compose-command above, access db directly with:
```bash
docker exec -it b2c_postgres psql -U b2c_user -d b2c_db
```

### Stop containers
To tear down containers and volumes (i.e. persistent saved data) run:

```bash
docker compose down -v
```

### Rebuild system from scratch
To rebuild system from scratch, run:
```bash
docker compose up --build --force-recreate --renew-anon-volumes
```

# Information on the use of GAI
During code writing of this entire project, AI assistance was used for troubleshooting issues and bugs in the code, suggesting overall coding solutions, refactoring code, providing tutoring, offering explanations of existing code, and recommending libraries and modules used during development. Simply stated, the code in this project can be said to have been written with AI assistance, but the authors have taken care to manually evaluate and review the code directly produced in this way.
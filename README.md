# B2C platform

## Docker setup
This project uses Docker Compose to run:
- PostgreSQL database (container name: b2c_postgres)
- React + Vite frontend (container name: b2c_frontend)

### Start all containers
To start all containers, run the command from the project root:
```bash
docker compose up --build
```
### Access the Data Base
After having run the compose-command above, access db directly with:
```bash
docker exec -it b2c_postgres psql -U b2c_user -d b2c_db
```

### Access the Frontend (Development)
Open in your browser:
```
http://localhost:5173
```

### Stop containers

To tear down containers and volumes (i.e. persistent saved data) run:

```bash
docker compose down -v
```

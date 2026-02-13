# B2C platform

## Docker containerization for db
Preliminary setup includes docker containerization for db. Set up containerized db with:

```bash
docker compose up db
```
After having run the compose-command above, access db directly with:

```bash
docker exec -it b2c_postgres psql -U b2c_user -d b2c_db
```

To tear down volumes in db (i.e. persistent saved data) run:

```bash
docker compose down -v
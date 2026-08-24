# F1 Race Platform

Ovaj repository se pokrece iz root foldera i podize kompletan sistem:

- `eventstore` .NET API
- `simulator` FastAPI aplikaciju
- SQL Server
- RabbitMQ

## Brzo pokretanje

Potreban je Docker Desktop sa ukljucenim Linux containers rezimom i najmanje 4 GB memorije dodeljene Docker-u.

Iz root foldera pokrenuti:

```powershell
Set-Location "F:\opms-aplikacija"
docker compose up --build --detach
```

Pri prvom pokretanju Docker preuzima slike i gradi oba servisa, pa komanda moze trajati nekoliko minuta. API automatski primenjuje EF Core migracije, a simulator kreira SQLite tabele.

Provera statusa:

```powershell
docker compose ps
```

`sqlserver`, `rabbitmq` i `simulator` treba da budu `healthy`, a `eventstore` treba da bude `Up`.

## Lokalni pristup servisima

| Servis | Adresa | Kredencijali |
| --- | --- | --- |
| Event Store Swagger | `http://localhost:5215/swagger` | Nema autentikacije |
| Simulator FastAPI dokumentacija | `http://localhost:8000/docs` | Nema autentikacije |
| RabbitMQ Management | `http://localhost:15672` | `eventstore` / `eventstore-password` |
| RabbitMQ AMQP | `amqp://eventstore:eventstore-password@localhost:5672/` | `eventstore` / `eventstore-password` |
| SQL Server | `localhost,1433` | `sa` / `EventStore!2026` |

Simulator objavljuje poruke na RabbitMQ direct exchange `simulatorExchange`, routing key `raceroute`. Event Store consumer prima poruke iz reda `raceevents`.

## Portovi i lokalne vrednosti

Podrazumevane vrednosti su u `.env.example`. `.env` fajl nije potreban kada su podrazumevani portovi slobodni.

Ako je neki port zauzet, napraviti lokalnu kopiju i izmeniti samo potrebne vrednosti:

```powershell
Copy-Item .env.example .env
```

Na primer, za postojeći RabbitMQ na hostu promeniti sledeće vrednosti u `.env`:

```text
RABBITMQ_AMQP_PORT=5673
RABBITMQ_MANAGEMENT_PORT=15673
```

Zatim ponovo kreirati kontejnere:

```powershell
docker compose up --detach --force-recreate
```

## Logovi i upravljanje

API logovi:

```powershell
docker compose logs --follow eventstore
```

Simulator logovi:

```powershell
docker compose logs --follow simulator
```

Zaustavljanje uz zadrzavanje podataka SQL Servera i RabbitMQ-a:

```powershell
docker compose down
```

Brisanje Docker podataka SQL Servera i RabbitMQ-a:

```powershell
docker compose down --volumes
```

Simulator koristi fajl `src/simulator/formula.db` direktno sa hosta. Taj fajl se cuva i kada se izvrsi `docker compose down --volumes`; za potpuno praznu simulator bazu potrebno ga je posebno obrisati ili zameniti rezervnom kopijom.

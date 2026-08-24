# F1 Event Store

## Pokretanje na laptopu

Potreban je Docker Desktop sa ukljucenim Linux containers rezimom i najmanje 4 GB memorije dodeljene Docker-u.

Iz ovog direktorijuma pokreni:

```powershell
docker compose up --build
```

Compose podize API, SQL Server i RabbitMQ. API automatski primenjuje EF Core migracije kada se SQL Server oznaci kao spreman.

- Swagger: `http://localhost:5215/swagger`
- RabbitMQ management: `http://localhost:15672`
- RabbitMQ AMQP: `amqp://eventstore:eventstore-password@localhost:5672/`
- SQL Server: `localhost,1433`, korisnik `sa`, lozinka `EventStore!2026`

Za promenu portova ili lokalnih lozinki kopiraj `.env.example` u `.env` i izmeni vrednosti pre prvog pokretanja. Lozinka za SQL Server mora da ispunjava SQL Server pravila za kompleksnost.

Za zaustavljanje servisa uz ocuvanje podataka:

```powershell
docker compose down
```

Za potpuno brisanje lokalne baze i RabbitMQ podataka:

```powershell
docker compose down --volumes
```

RabbitMQ consumer slusa red `raceevents` na direct exchange-u `simulatorExchange` sa routing key-jem `raceroute`.

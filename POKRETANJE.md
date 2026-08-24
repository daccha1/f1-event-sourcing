# Pokretanje F1 Event Store aplikacije

Ovo uputstvo podize kompletan lokalni sistem:

- `eventstore` API
- SQL Server bazu `F1EventStoreDb`
- RabbitMQ broker i management interfejs

## Preduslovi

1. Instalirati Docker Desktop.
2. U Docker Desktop-u ukljuciti Linux containers rezim.
3. Dodeliti Docker-u najmanje 4 GB memorije.
4. Proveriti da su slobodni portovi `5215`, `1433`, `5672` i `15672`.

## Prvo pokretanje

Otvoriti PowerShell u folderu projekta:

```powershell
Set-Location "F:\opms-aplikacija\src\event-store\eventstore"
```

Pokrenuti sve servise:

```powershell
docker compose up --build --detach
```

Pri prvom pokretanju Docker preuzima .NET, SQL Server i RabbitMQ slike, zato komanda moze trajati nekoliko minuta. API automatski kreira bazu i primenjuje EF Core migracije nakon sto SQL Server postane spreman.

Proveriti status:

```powershell
docker compose ps
```

`sqlserver` i `rabbitmq` treba da imaju status `healthy`, a `eventstore` status `Up`.

## Adrese i kredencijali

| Servis              | Adresa                                                  | Kredencijali                         |
| ------------------- | ------------------------------------------------------- | ------------------------------------ |
| Swagger API         | `http://localhost:5215/swagger`                         | Nema autentikacije                   |
| RabbitMQ Management | `http://localhost:15672`                                | `eventstore` / `eventstore-password` |
| RabbitMQ AMQP       | `amqp://eventstore:eventstore-password@localhost:5672/` | `eventstore` / `eventstore-password` |
| SQL Server          | `localhost,1433`                                        | `sa` / `EventStore!2026`             |

RabbitMQ consumer cita red `raceevents` sa direct exchange-a `simulatorExchange` i routing key-ja `raceroute`.

## Promena lokalnih portova ili lozinki

Compose ima podrazumevane development vrednosti, tako da `.env` fajl nije obavezan. Kada je neki port zauzet, napraviti lokalnu kopiju konfiguracije:

```powershell
Copy-Item .env.example .env
```

Zatim u `.env` promeniti potrebne vrednosti. Na primer, ako je port `5672` vec zauzet:

```text
RABBITMQ_AMQP_PORT=5673
RABBITMQ_MANAGEMENT_PORT=15673
```

Nakon izmene portova ponovo kreirati kontejnere:

```powershell
docker compose up --detach --force-recreate
```

## Logovi i ponovno pokretanje

Log API-ja:

```powershell
docker compose logs --follow eventstore
```

Logovi svih servisa:

```powershell
docker compose logs --follow
```

Ponovno pokretanje samo API-ja:

```powershell
docker compose restart eventstore
```

## Gasenje i reset

Zaustaviti kontejnere uz zadrzavanje baze i RabbitMQ podataka:

```powershell
docker compose down
```

Za potpuno brisanje lokalnih podataka i sledece pokretanje sa praznom bazom:

```powershell
docker compose down --volumes
```

## Pokretanje na novom laptopu

1. Prebaciti ceo folder `eventstore` na laptop.
2. Instalirati i pokrenuti Docker Desktop.
3. Otvoriti PowerShell u prebacenom folderu.
4. Pokrenuti `docker compose up --build --detach`.
5. Otvoriti Swagger na `http://localhost:5215/swagger`.

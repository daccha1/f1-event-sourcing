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

| Servis                          | Adresa                                                  | Kredencijali                         |
| ------------------------------- | ------------------------------------------------------- | ------------------------------------ |
| Event Store Swagger             | `http://localhost:5215/swagger`                         | Nema autentikacije                   |
| Simulator FastAPI dokumentacija | `http://localhost:8000/docs`                            | Nema autentikacije                   |
| RabbitMQ Management             | `http://localhost:15672`                                | `eventstore` / `eventstore-password` |
| RabbitMQ AMQP                   | `amqp://eventstore:eventstore-password@localhost:5672/` | `eventstore` / `eventstore-password` |
| SQL Server                      | `localhost,1433`                                        | `sa` / `EventStore!2026`             |
| Redis                           | `localhost:6379`                                        | Nema autentikacije                   |

Simulator objavljuje poruke na RabbitMQ direct exchange `simulatorExchange`, routing key `raceroute`. Event Store consumer prima poruke iz reda `raceevents`.

## Kesiranje statistike (Redis)

Tabele sampionata se ne cuvaju kao gotovo stanje nego se svaki put racunaju ponovo iz event stream-a.
Taj racun je najskuplja operacija za citanje u sistemu, pa stoji iza Redis kesa.

Invalidacija ide preko generacijskog brojaca u Redisu (`cache:version:standings`). Kljucevi kesa u sebi
nose trenutnu verziju, pa upis jednog eventa povlaci celu grupu unosa odjednom, bez brisanja pojedinacnih
kljuceva. Brojac se uvecava samo za evente koje projekcija stvarno cita (`StartedRace` i `FinishedRace`) --
preticanja, pit stopovi i krasevi ne obaraju kes. Zbog toga jedna trka od ~800 eventa podigne verziju
samo ~38 puta.

Ako Redis nije dostupan, API se i dalje podize i citanja padaju na bazu.

### Endpointi za poredjenje

Svaki keširani endpoint ima blizanca koji uvek racuna iz baze, radi merenja:

| Endpoint                                     | Izvor                       |
| -------------------------------------------- | --------------------------- |
| `GET /api/statistics/scoreboard`             | Redis kes                   |
| `GET /api/statistics/scoreboard/no-cache`    | Uvek iz baze (event replay) |
| `GET /api/statistics/constructors`           | Redis kes                   |
| `GET /api/statistics/constructors/no-cache`  | Uvek iz baze (event replay) |

Oba endpointa racunaju kroz istu projekciju, pa ne mogu da se raziju u rezultatu.

Odgovor nosi zaglavlja koja govore kako je nastao:

- `X-Data-Source` -- `cache` ili `database`
- `X-Cache` -- `HIT`, `MISS` (kes promasen, projekcija se racunala) ili `BYPASS` (no-cache endpoint)
- `X-Elapsed-Ms` -- vreme obrade na serveru
- `X-Cache-Version` -- generacija kesa koriscena za kljuc

Poredjenje iz komandne linije:

```powershell
curl.exe -s -o NUL -D - http://localhost:5215/api/statistics/scoreboard/no-cache
curl.exe -s -o NUL -D - http://localhost:5215/api/statistics/scoreboard
```

Prvi poziv keširanog endpointa posle upisa novog eventa vraca `X-Cache: MISS`, svaki sledeci `HIT`.

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
REDIS_PORT=6380
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

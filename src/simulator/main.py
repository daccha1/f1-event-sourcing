# # from fastapi import FastAPI, Depends
# # import Data
# # from Data.models import Intern
# # from Repository import foos
# #
# # from Data.database import  SessionLocal
# # from sqlalchemy.orm import Session
# #
# # def get_db():
# #     db = SessionLocal()
# #     try:
# #         yield db
# #     finally:
# #         db.close()
# #
# # app = FastAPI()
# #
# #
# # @app.get('/dbtest', response_model=[Data.schemas.User])
# # async def dbtest(db: Session = Depends(get_db)):
# #     user = Intern()
# #     user.Email = "petar@gmail.com"
# #     db.add()
# #
# # @app.get('/dbinsert')
# # async def dbinsert(db: Session = Depends(get_db)):
# #     intern = Data.models.Intern()
# #
# #
# #
# #
# # @app.get('/')
# # async def index():
# #     return foos.getContainer()
# #
# # @app.post('/new')
# # async def newUser():
# #     intern = Intern(
# #         id = 1,
# #         email = "darko@gmail.com",
# #         position="Intern - Vajber",
# #         ugovor = False,
# #         started = "nbt",
# #         username = "darko"
# #     )
# #     foos.container.append(intern)
# #
# # @app.get('/path')
# # async def pathTestFoo(size: int):
# #     return {"size is": size}
# #
# # @app.post('/new/intern')
# # async def newUserIntern(intern: Intern):
# #     foos.container.append(intern)
# from Models.models import Race, Driver
# from Repository.racingFunctions import raceStart
# races = []
# drivers = []
# crashed_drivers = []
#
# monaco = Race(
#     id = 1,
#     name = "Monaco",
#     laps=60,
#     length=1.53,
# )
# monaco.pit_setup()
#
# rbr = Race(
#     id = 2,
#     name = "RedBull Ring",
#     laps=50,
#     length=1.53,
# )
# rbr.pit_setup()
#
# races.append(monaco)
# races.append(rbr)
#
# max = Driver(
#     id = 1,
#     name = "Max Verstappen",
#     team="RedBull",
#     racing_coeff=4.8,
# )
#
# lec = Driver(
#     id = 2,
#     name = "Charles Leclerc",
#     team="Ferrari",
#     racing_coeff=4.3,
# )
#
# seinz = Driver(
#     id = 3,
#     name = "Carlos Seinz",
#     team="Williams",
#     racing_coeff=3.5,
# )
# alo = Driver(
#     id = 4,
#     name = "Fernando Alonso",
#     team="Aston Martin",
#     racing_coeff=3,
# )
# alb = Driver(
#     id = 4,
#     name = "Alex Albon",
#     team="Williams",
#     racing_coeff=3.3,
# )
# drivers.append(max)
# drivers.append(lec)
# drivers.append(seinz)
# drivers.append(alo)
# drivers.append(alb)
#
# raceStart(races[0], drivers, crashed_drivers)
#
# print()
# print('Crashed:')
# for driver in crashed_drivers:
#     print(driver.name)

from fastapi import FastAPI, Depends
from Data.dbSchemas import Race, Driver
from Data.database import SessionLocal, get_db, Base, engine
from typing import Annotated
from sqlalchemy.orm import Session
from Models.models import DriverRequest, DriverResponse, RaceRequest, RaceResponse, RacingDriver
from typing import List
from Repository.racingFunctions import raceStart

dbdep = Annotated[Session, Depends(get_db)]

Base.metadata.create_all(bind=engine)

app = FastAPI()

@app.get('/races', response_model=List[RaceResponse])
async def get_races(db: dbdep):
    return db.query(Race).all()

@app.post('/races/new', response_model=RaceResponse)
async def add_race(db:dbdep, race: RaceRequest):
    db_race = Race(**race.model_dump())
    db.add(db_race)
    db.commit()
    db.refresh(db_race)
    return race

@app.get('/drivers', response_model=List[DriverResponse])
async def get_drivers(db: dbdep):
    return db.query(Driver).all()

@app.post('/drivers/new', response_model=DriverResponse)
async def add_driver(db:dbdep, driver: DriverRequest):
    db_driver = Driver(**driver.model_dump())

    db.add(db_driver)
    db.commit()
    db.refresh(db_driver)
    return db_driver


@app.post('/race/start')
def start_race(db: dbdep, seed: int | None = None):
    circuit = db.query(Race).all()
    selected_circuit = circuit[1]
    grid = [
        RacingDriver(
            driver_id=d.id,
            name=d.name,
            team=d.team,
            racing_coeff=d.racing_coeff,
            crash_coeff=d.crash_coeff,
            pit_coeff=d.pit_coeff,
        )
        for d in db.query(Driver).all()
    ]
    crashed_drivers: list[RacingDriver] = []
    classification = raceStart(selected_circuit, grid, crashed_drivers, seed=seed)

    return {
        'race': selected_circuit.name,
        'seed': seed,
        'classification': [
            {'position': d.finished_position, 'driver_id': d.driver_id, 'name': d.name}
            for d in classification
        ],
        'dnf': [
            {'driver_id': d.driver_id, 'name': d.name, 'lap': d.crashed_on_lap}
            for d in crashed_drivers
        ],
    }



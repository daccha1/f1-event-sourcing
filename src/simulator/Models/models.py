from dataclasses import dataclass
from pydantic import BaseModel, ConfigDict
from math import floor

class RaceRequest(BaseModel):
    name: str
    laps: int
    length: float
    pit_number: int = 1  # laps/20
    pit_after: int = 1  # pit after N laps

    def pit_setup(self):
        self.pit_number = floor(self.laps/20)
        self.pit_after  = 20

class RaceResponse(BaseModel):
    id: int
    name: str
    laps: int
    length: float
    pit_number: int = 1  # laps/20
    pit_after: int = 1  # pit after N laps

    model_config = ConfigDict(from_attributes=True)

class DriverRequest(BaseModel):
    name: str
    team: str
    racing_coeff: float


class DriverResponse(BaseModel):
    id: int
    name: str
    team: str
    racing_coeff: float
    pit_coeff: float = 0.05
    crash_coeff: float = 0.0001
    pits: int = 0

    model_config = ConfigDict(from_attributes=True)


# Runtime kopija vozaca za vreme trke, da simulacija ne menja ORM entitete.
# eq=False: poredjenje po identitetu, da `list.remove()` ukloni bas taj objekat.
@dataclass(eq=False)
class RacingDriver:
    driver_id: int
    name: str
    team: str
    racing_coeff: float
    crash_coeff: float = 0.0001
    pit_coeff: float = 0.05
    pits: int = 0
    finished_position: int | None = None
    crashed_on_lap: int | None = None

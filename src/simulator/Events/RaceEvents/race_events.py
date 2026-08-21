from pydantic import BaseModel, Field
from uuid import UUID

# Race events that will be converted into JSON format

class FinishRace(BaseModel):
    raceId: UUID

class CreateRace(BaseModel):
    raceId: UUID
    country: str
    gp: str
    laps: int

class StopRace(BaseModel):
    raceId: UUID
    lap: int


import uuid
from datetime import datetime
from enum import Enum

from pydantic import BaseModel, Field


class EventType(Enum):
    RACE_ERROR = -1,
    RACE_STARTED = 0
    #OVERTAKE
    #DRIVER_CRASHED
    #DRIVER_PITSTOP
    #DRIVER_FINISHED

class EventWrapper(BaseModel):
    CorrelationId : str = Field(default_factory=lambda: str(uuid.uuid4()))
    Payload : str
    EventType : str
    OcurredAt : datetime



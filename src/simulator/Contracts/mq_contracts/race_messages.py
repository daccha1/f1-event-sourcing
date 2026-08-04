from enum import Enum
import uuid
from pydantic import Field

from pydantic import BaseModel


class EventType(Enum):
    RACE_ERROR = -1,
    RACE_STARTED = 0
    #OVERTAKE
    #DRIVER_CRASHED
    #DRIVER_PITSTOP
    #DRIVER_FINISHED

class RaceMessage(BaseModel):
    id: str = Field(default_factory=lambda: uuid.uuid4().hex)
    type: EventType
    # JSON string (deserializes based on the EventType)
    payload: str = ""

start_evt = RaceMessage(
    type=EventType.RACE_STARTED
)


from pydantic import BaseModel
from typing import Optional, Any
from aiohttp import ClientSession


class LoginForm(BaseModel):
    username: str
    password: str


class LoginResult:
    def __init__(
        self,
        state: bool,
        status_code: int = 200,
        message: Optional[str] = None,
        session: Optional[ClientSession] = None,
    ):
        self.state = state
        self.status_code = status_code
        self.message = message
        self.session = session


class StandardResponse(BaseModel):
    status_code: int
    error_msg: Optional[str] = None
    data: Optional[Any] = None

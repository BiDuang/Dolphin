from pydantic import BaseModel
from typing import List, Optional,Any
from datetime import datetime


class Account(BaseModel):
    uid: str
    email: str
    uri: str
    notice_day: List[int]
    noticed_info: List[Any]


class Task(BaseModel):
    title: str
    time: datetime
    id: str
    notice_type: int


class UserInfo(BaseModel):
    uid: str
    name: str
    college: str
    major: str

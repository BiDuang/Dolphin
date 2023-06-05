import json, base64, ics
from aiohttp import ClientSession
from typing import List
from datetime import datetime, timedelta

from Utils.database import AsyncMySQL
from Utils.encrypt import OUCPasswordEncrypt
from Models.request import LoginForm, LoginResult
from Models.user import Account, Task


async def PortalLogin(body: LoginForm) -> LoginResult:
    try:
        session = ClientSession()

        await session.get("http://id.ouc.edu.cn:8071/sso/login")

        resp = await session.post(
            "http://id.ouc.edu.cn:8071/sso/ssoLogin",
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            data={
                "username": body.username,
                "password": OUCPasswordEncrypt(body.password),
            },
        )

        assert json.loads(await resp.read())["state"]

        await session.post(
            "http://id.ouc.edu.cn:8071/sso/login?service=http://my.ouc.edu.cn/user/simpleSSOLogin",
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            data={
                "username": body.username,
                "password": base64.b64encode(body.password.encode()).decode(),
                "lt": "e2s1",
                "_eventId": "submit",
            },
        )

        resp = await session.post(
            "http://id.ouc.edu.cn:8071/sso/login?service=http://my.ouc.edu.cn/user/simpleSSOLogin",
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            data={
                "username": body.username,
                "password": base64.b64encode(body.password.encode()).decode(),
                "lt": "e2s1",
                "_eventId": "submit",
            },
        )

        return LoginResult(state=True, session=session)

    except AssertionError:
        return LoginResult(
            state=False, status_code=401, message="Wrong username or password"
        )
    except Exception as e:
        return LoginResult(state=False, status_code=500, message=str(e))


async def GetAccounts() -> List[Account]:
    db = AsyncMySQL()
    await db.init()
    res = await db.search("UID, Email, URI, Notice_Day, Noticed_Info", "ics_links", "1")
    await db.close()
    return [
        Account(
            uid=i[0],
            email=i[1],
            uri=i[2],
            notice_day=i[3].split(","),
            noticed_info=json.loads(i[4]),
        )
        for i in res
    ]


async def GetFutureTask(user: Account) -> List[Task]:
    async with ClientSession() as session:
        async with session.get(user.uri) as resp:
            ics_content = await resp.text()
            ics_calendar = ics.Calendar(ics_content)
            tasks = []
            for i in ics_calendar.timeline:
                for daysdelta in user.notice_day:
                    if i.begin.datetime.replace(
                        tzinfo=None
                    ) - datetime.now() >= timedelta(
                        days=0
                    ) and i.begin.datetime.replace(
                        tzinfo=None
                    ) - datetime.now() <= timedelta(
                        days=daysdelta
                    ):
                        if daysdelta == 1 and i.uid[16:] in user.noticed_info[0]:
                            continue
                        elif daysdelta == 3 and i.uid[16:] in user.noticed_info[1]:
                            continue
                        elif daysdelta == 7 and i.uid[16:] in user.noticed_info[2]:
                            continue
                        tasks.append(
                            Task(
                                title=i.name,
                                time=i.begin.datetime,
                                notice_type=daysdelta,
                                id=i.uid[16:],
                            )
                        )
            return tasks


async def UpdateNoticedRecord(user: Account) -> None:
    db = AsyncMySQL()
    await db.init()
    info = json.dumps(user.noticed_info)
    await db.update(
        "ics_links",
        f"`Noticed_Info`='{info}'",
        f'Email="{user.email}"',
    )
    await db.close()

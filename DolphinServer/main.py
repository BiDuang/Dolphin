from fastapi import FastAPI, HTTPException, Response
from fastapi.middleware import cors
from bs4 import BeautifulSoup
import base64, json

from Utils.requests import PortalLogin
from Utils.database import AsyncMySQL
from Models.request import LoginForm, StandardResponse
from Models.user import Account, UserInfo

from monitor import Monitor

app = FastAPI()

app.add_middleware(
    cors.CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.on_event("startup")
async def Init():
    global db, sub
    db = AsyncMySQL()
    await db.init()
    sub = Monitor()
    sub.start()


@app.on_event("shutdown")
async def shutdown():
    await db.close()
    sub.Flag = False
    sub.join()


@app.post("/register")
async def Register(user: Account):
    res = await db.search("*", "ics_links", f'Email="{user.email}"', "1")

    if res == ():
        await db.insert(
            "ics_links",
            "UID, Email, URI, Notice_Day, Noticed_Info",
            f'"{user.uid}","{user.email}","{user.uri}","{user.notice_day.__str__()[1:-1].replace(" ", "")}","[[],[],[]]"',
        )
    else:
        await db.update(
            "ics_links",
            f'Email="{user.email}", URI="{user.uri}", Notice_Day="{user.notice_day.__str__()[1:-1].replace(" ", "")}"',
            f'UID="{user.uid}"',
        )

    return Response(status_code=201)


@app.delete("/quit")
async def Quit(uid:str):
    await db.delete("ics_links", f'UID="{uid}"')
    return Response(status_code=200)


@app.post("/homework", response_model=StandardResponse)
async def GetHomeworkSchedule(body: LoginForm):
    result = await PortalLogin(body)
    if not result.state:
        raise HTTPException(status_code=result.status_code, detail=result.message)

    assert result.session is not None

    resp = await result.session.post(
        "http://my.ouc.edu.cn/c/portal/baseUserInfoAction",
        headers={"Referer": "http://my.ouc.edu.cn/web/guest"},
    )

    user_json = json.loads(await resp.read())

    resp = await result.session.post(
        "http://id.ouc.edu.cn:8071/sso/login?service=https://wlkc.ouc.edu.cn/webapps/bb-sso-BBLEARN/index.jsp",
        headers={"Content-Type": "application/x-www-form-urlencoded"},
        data={
            "username": body.username,
            "password": base64.b64encode(body.password.encode()).decode(),
            "lt": "e2s1",
            "_eventId": "submit",
        },
    )

    token = BeautifulSoup(await resp.text(), "html.parser").find("input", {"name": "token"}).get("value")  # type: ignore

    await result.session.post(
        "https://wlkc.ouc.edu.cn/webapps/bb-sso-BBLEARN/execute/authValidate/customLogin",
        headers={"Content-Type": "application/x-www-form-urlencoded"},
        data={
            "username": body.username,
            "token": token,
        },
    )

    resp = await result.session.get(
        "https://wlkc.ouc.edu.cn/webapps/calendar/calendarFeed/url"
    )

    schedule = await resp.text()
    await result.session.close()
    return StandardResponse(
        status_code=200,
        data=[
            schedule,
            UserInfo(
                uid=user_json["workId"],
                name=user_json["name"],
                college=user_json["firstLevelUnit"],
                major=user_json["secondLevelUnit"],
            ),
        ],
    )

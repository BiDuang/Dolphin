import asyncio, os, aiosmtplib, time, threading
from email.mime.text import MIMEText
from datetime import datetime

if not os.path.exists(os.path.join(os.getcwd(), "Configs", "config.py")):
    raise Exception("Required Config is not set")
from Configs.config import MAIL_CONFIG
from Utils.requests import GetAccounts, GetFutureTask, UpdateNoticedRecord


class Monitor(threading.Thread):
    Flag = True

    def run(self):
        while self.Flag:
            asyncio.run(self.MonitorJob())
            if self.Flag:
                time.sleep(600)

    async def MonitorJob(self) -> None:
        print(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')} - Running MonitorJob")
        accounts = await GetAccounts()
        for account in accounts:
            tasks = await GetFutureTask(account)
            if len(tasks) == 0:
                continue
            print(f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')} - Sending Message")
            f = open(
                os.path.join(os.getcwd(), "Mails", "captcha.html"),
                "r",
                encoding="utf-8",
            )
            msg = f.read()
            f.close()
            task_info = ""
            for task in tasks:
                task_info += f"<li>{task.title} - 截止于: {task.time.strftime('%Y-%m-%d %H:%M:%S')}</li>"
            msg = msg.replace("{{ .TaskInfo }}", task_info)
            message = MIMEText(msg, "html", "utf-8")
            message["From"] = MAIL_CONFIG["username"]
            message["To"] = account.email
            message["Subject"] = "[Dolphin] BB 平台作业日程提醒"

            await aiosmtplib.send(
                message,
                hostname=MAIL_CONFIG["host"],
                port=MAIL_CONFIG["port"],
                start_tls=True,
                username=MAIL_CONFIG["username"],
                password=MAIL_CONFIG["password"],
            )

            for task in tasks:
                if task.notice_type == 1:
                    account.noticed_info[0].append(task.id)
                elif task.notice_type == 3:
                    account.noticed_info[1].append(task.id)
                elif task.notice_type == 7:
                    account.noticed_info[2].append(task.id)

            await UpdateNoticedRecord(account)

            print(
                f"{datetime.now().strftime('%Y-%m-%d %H:%M:%S')} - Message to {account.email} send"
            )

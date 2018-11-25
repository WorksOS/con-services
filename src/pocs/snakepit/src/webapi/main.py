from flask import Flask
from flask import request
from flask_cors import CORS
from datetime import datetime
from src.httpClients import SchedulerClient, PassCountExport
import requests
app= Flask(__name__)
CORS(app)

@app.route("/")
def timestamp():
    return "Welcome to the snakepit, there are {} snakes here".format(datetime.now().timestamp())


@app.route("/export", methods=["GET"])
def export():
    args = request.args
    auth_headers = {"X-VisionLink-CustomerUid": request.headers["X-VisionLink-CustomerUid"],
                         "Authorization": request.headers["Authorization"]}
    client = SchedulerClient()
    res = client.schedule_veta_export(args, auth_headers)
    job_id = res.json()["jobId"]

    export_status = client.poll_job_until_complete(job_id, auth_headers)
    if export_status.status == requests.codes.ok and \
        export_status.content.json()["stauts"] == "Succeeded":
        pass_count_export = PassCountExport(export_status.content.json["downloadLink"])

    return export_status.content





if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0')

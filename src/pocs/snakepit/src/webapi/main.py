from flask import Flask, send_file
from flask import request
from flask_cors import CORS
from datetime import datetime
from src.httpClients import SchedulerClient, VetaExport
from src.landfill import LandfillAlgorithm
import requests
import json
import logging as log
app= Flask(__name__)
CORS(app)

log.basicConfig(level=log.DEBUG)

@app.route("/")
@app.route("/ping")
def timestamp():
    return "Welcome to the snakepit, there are {} snakes here".format(datetime.now().timestamp())



@app.route("/export", methods=["GET"])
def export():
    args = request.args
    log.debug("Export request {} received".format(args))
    auth_headers = {"X-VisionLink-CustomerUid": request.headers["X-VisionLink-CustomerUid"],
                         "Authorization": request.headers["Authorization"]}
    client = SchedulerClient()

    res = client.schedule_veta_export(args, auth_headers)
    if not res.status_code == requests.codes.ok:
        return res.content, res.status_code
    job_id = res.json()["jobId"]

    export_status = client.poll_job_until_complete(job_id, auth_headers)
    if export_status.status_code == requests.codes.ok and \
        json.loads(export_status.content)["status"] == "Succeeded":
        pass_count_export = VetaExport(json.loads(export_status.content)["downloadLink"])
        landfill_algorithm = LandfillAlgorithm()
        log.debug("Generating snakepit report")
        landfill_report = landfill_algorithm.generate_report(pass_count_export.get_veta_export())
        log.debug("Snakepit report generated")
        landfill_report.seek(0)
        log.debug("Returning generated report")
        return send_file(landfill_report,
                         attachment_filename="Landfill Report",
                         mimetype="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    log.critical("Report generation failed")
    return "Export failed", export_status.status_code


if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0')

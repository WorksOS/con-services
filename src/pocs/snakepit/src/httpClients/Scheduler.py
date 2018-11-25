import requests

class SchedulerClient():

    def __init__(self):
        self.scheduleExportUrl = "https://api-stg.trimble.com/t/trimble.com/vss-dev-3dproductivity/2.0/export/machinepasses/schedulejob"
        self.shceduler_service_url = "https://api-stg.trimble.com/t/trimble.com/vss-dev-3dscheduler/1.0"
        self.job_complete_status = ["Succeeded", "Failed"]

    def schedule_veta_export(self, veta_export_request, auth_headers):

        result = requests.get(self.scheduleExportUrl, params=veta_export_request, headers=auth_headers)
        return result

    def get_secheduler_job_status(self, job_id, auth_headers):
        job_url = "{}/export/{}".format(self.shceduler_service_url, job_id)
        result = requests.get(job_url, headers=auth_headers)
        return result

    def poll_job_until_complete(self, job_id, auth_headers):
        status = self.get_secheduler_job_status(job_id, auth_headers)
        while status.status_code == requests.codes.ok and status.content.json()["status"] not in self.job_complete_status:
            status = self.get_secheduler_job_status(job_id, auth_headers)

        return status




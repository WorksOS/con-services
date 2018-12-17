import requests
import json
import logging as log

class SchedulerClient():

    def __init__(self):
        #self.scheduleExportUrl = "https://api-stg.trimble.com/t/trimble.com/vss-dev-3dproductivity/2.0/export/veta/schedulejob"
        #self.scheduler_service_url = "https://api-stg.trimble.com/t/trimble.com/vss-dev-3dscheduler/1.0"
        self.scheduleExportUrl = "https://3dproductivity.myvisionlink.com/t/trimble.com/vss-3dproductivity/2.0/export/veta/schedulejob"
        self.scheduler_service_url = "https://3dproductivity.myvisionlink.com/t/trimble.com/vss-3dscheduler/1.0"



        self.job_complete_status = ["Succeeded", "Failed"]

    def schedule_veta_export(self, veta_export_request, auth_headers):
        log.debug("Scheduling veta export {}".format(veta_export_request))
        result = requests.get(self.scheduleExportUrl, params=veta_export_request, headers=auth_headers)
        log.debug("Veta export schedule result {}".format(result))
        return result

    def get_secheduler_job_status(self, job_id, auth_headers):
        job_url = "{}/export/{}".format(self.scheduler_service_url, job_id)
        log.info("Checking job status for job id {}".format(job_id))
        result = requests.get(job_url, headers=auth_headers)
        return result

    def poll_job_until_complete(self, job_id, auth_headers):
        log.debug("Waiting for veta export to complete")
        status = self.get_secheduler_job_status(job_id, auth_headers)
        while status.status_code == requests.codes.ok and json.loads(status.content)["status"] not in self.job_complete_status:
            status = self.get_secheduler_job_status(job_id, auth_headers)

        log.debug("Veta export {} completed with result {}".format(job_id, status))
        return status




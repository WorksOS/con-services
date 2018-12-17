import requests
import io
import tempfile
from zipfile import ZipFile
import logging as log
class VetaExport():

    def __init__(self, report_location):
        self.report_location = report_location

    def get_veta_export(self):
        log.debug("Downloading veta export from s3")
        downloaded = self._download_report()
        extracted_files = {}
        if downloaded.status_code == requests.codes.ok:
            log.debug("Downloaded ok, starting to extract")
            zip_file = ZipFile(io.BytesIO(downloaded.content))
            for file in zip_file.namelist():
                log.debug("extracting {}".format(file))
                temp = tempfile.SpooledTemporaryFile(max_size=100000, mode="w+b")
                temp.write(zip_file.read(file))
                temp.seek(0)
                extracted_files[file] = temp
        log.debug("extraction finished extracted count is {}".format(len(extracted_files)))
        return extracted_files

    def _download_report(self):
        return requests.get(self.report_location)

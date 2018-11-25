import requests
import io
import tempfile
from zipfile import ZipFile
class PassCountExport():

    def __init__(self, report_location):
        self.report_location = report_location

    def get_passcount_export(self):
        downloaded = self._download_report()
        extracted_files = {}
        if downloaded.status_code == requests.codes.ok:
            zip_file = ZipFile(io.BytesIO(downloaded.content))
            for file in zip_file.namelist():
                temp = tempfile.SpooledTemporaryFile(max_size=1000, mode="w+b")
                temp.write(zip_file.read(file))
                temp.seek(0)
                extracted_files[file] = temp
        return extracted_files

    def _download_report(self):
        return requests.get(self.report_location)

import { Component, Inject } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';

// const URL = '/api/';
const URL = 'http://trex.dev.k8s.vspengg.com/api/v2/tagfiles';
const ProjectUid = 'bea52e4f-faa2-e511-80e5-0050568821e6';
const OrgId = "87bdf851-44c5-e311-aa77-00505688274d";


@Component({
  selector: 'upload-data',
  templateUrl: './upload-data.component.html'
})
export class UploadDataComponent {
  public uploader: FileUploader = new FileUploader({
    url: URL,
    disableMultipart: true,
    formatDataFunctionIsAsync: true,
    formatDataFunction: async (item: any) => {
      return new Promise((resolve, reject) => {
        resolve({
          filename: item._file.name,
          data: item._data,
          ProjectUid: ProjectUid,
          OrgId: OrgId
        });
      });
    }
  });
  public hasBaseDropZoneOver: boolean = false;
  public hasAnotherDropZoneOver: boolean = false;






  public fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  public fileOverAnother(e: any): void {
    this.hasAnotherDropZoneOver = e;
  }
}
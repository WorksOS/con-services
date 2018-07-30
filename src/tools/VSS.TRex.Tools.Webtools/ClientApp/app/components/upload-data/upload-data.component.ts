import { Component, Inject } from '@angular/core';
import {  FileUploader } from 'ng2-file-upload';

// const URL = '/api/';
const URL = 'http://trex.dev.k8s.vspengg.com/api/v2/tagfiles';

@Component({
  selector: 'upload-data',
  templateUrl: './upload-data.component.html'
})
export class UploadDataComponent {
  public uploader: FileUploader = new FileUploader({ url: URL });
  public hasBaseDropZoneOver: boolean = false;
  public hasAnotherDropZoneOver: boolean = false;

  public fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  public fileOverAnother(e: any): void {
    this.hasAnotherDropZoneOver = e;
  }
}
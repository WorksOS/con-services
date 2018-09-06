import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';


import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ITagFileRequest } from './tagfile-request';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'x-visionlink-customeruid': '00000',
    'authorization': 'dont care'
  })
};

@Injectable()
export class UploadDataService {
  tagUploadUrl = 'http://trex.mutable.dev.k8s.vspengg.com/api/v2/tagfiles';  // URL to web api
  private handleError: HandleError;

  constructor(
    private http: HttpClient,
    httpErrorHandler: HttpErrorHandler) {
    this.handleError = httpErrorHandler.createHandleError('UploadDataService');
  }

  /** POST: submit a tag file to trex */
  addTagfile(tagFile: ITagFileRequest): Observable<ITagFileRequest> {
    return this.http.post<ITagFileRequest>(this.tagUploadUrl, tagFile, httpOptions)
      .pipe(
        catchError(this.handleError('submitTagFile', tagFile))
      );
  }
}


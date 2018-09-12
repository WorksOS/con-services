import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { strict } from 'assert';

import { ProjectExtents } from "../project/project-model"

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class SandboxService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('SandboxService');
    this.baseUrl = baseUrl;
  }

  public getProjectExtents(projectUid: string): Observable<ProjectExtents> {
    let url = `${this.baseUrl}api/sitemodels/${projectUid}/extents`;
    console.log(url);
    return this.http.get<ProjectExtents>(url);
  }
}

import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders, } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { ISiteModelMetadata } from '../project/project-model';
import { RebuildSiteModelRequestResponse } from './rebuild-project-model';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class RebuildProjectService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('RebuildProjectService');
    this.baseUrl = baseUrl;
  }

  private executeRequest<T>(label: string, url: string): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.get<T>(url);
  }

  private executePutRequest<T>(label: string, url: string): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.put<T>(url, null);
  }

  private executePostRequest<T>(label: string, url: string, body: any): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.post<T>(url, body);
  }

  private executeDeleteRequest<T>(label: string, url: string): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.delete<T>(url);
  }

  public rebuildProject(projectUid: string): Observable<RebuildSiteModelRequestResponse> {
    return this.executePostRequest<RebuildSiteModelRequestResponse>('rebuildProject', `sitemodels/${projectUid}/rebuild`, null);
  }

  public getAllProjectMetadata(): Observable<ISiteModelMetadata[]> {
    return this.executeRequest<ISiteModelMetadata[]>('getAllProjectMetadata', `sitemodels/metadata`);
  }
}

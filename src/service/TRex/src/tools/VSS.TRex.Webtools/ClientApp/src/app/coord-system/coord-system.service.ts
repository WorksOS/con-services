import { Injectable, Inject } from '@angular/core';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CoordinateSystemFile } from './coord-system.-model';

@Injectable()
export class CoordSystemService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('CoordSystemService');
    this.baseUrl = baseUrl;
  }

  private executeRequest<T>(label: string, url: string): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.get<T>(url);
  }

  private executePostRequest<T>(label: string, url: string, body: any): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.post<T>(url, body);
  }

  public getCoordSystemSettings(projectUid: string): Observable<string> {
    return this.executeRequest<string>("getCoordSystemSettings", `projects/${projectUid}/coordsystem`);
  }

  public addUpdateCoordSystem(csFile: CoordinateSystemFile): Observable<string> {
    return this.executePostRequest("addUpdateCoordSystem", "coordsystem", csFile);
  };
}
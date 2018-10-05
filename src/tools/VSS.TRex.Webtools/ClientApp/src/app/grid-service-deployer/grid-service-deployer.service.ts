import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { strict } from 'assert';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class GridServiceDeployerService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('GridServiceDeployerService');
    this.baseUrl = baseUrl;
  }

  public deployTagFileBufferQueueService(): void {
    let url = `${this.baseUrl}api/services/tagfilebuffer`;
    console.log(url);
    this.http.put<string>(url, null).subscribe((result) => {
      console.log(result);
    });
  }

  public deployMutableSegmentRetirementQueueService(): void {
    let url = `${this.baseUrl}api/services/segmentretirement/mutable`;
    console.log(url);
    this.http.put<string>(url, null).subscribe((result) => {
      console.log(result);
    });
  }
    
  public deployImmutableSegmentRetirementQueueService(): void {
    let url = `${this.baseUrl}api/services/segmentretirement/immutable`;
    console.log(url);
    this.http.put<string>(url, null).subscribe((result) => {
      console.log(result);
    });
  }
}

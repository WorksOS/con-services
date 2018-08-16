import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { strict } from 'assert';

import { IGrid } from './grid-model'

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class GridStatusService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('GridStatusService');
    this.baseUrl = baseUrl;
  }


  public getGrids(): IGrid[] {
    let url = this.baseUrl + 'api/grids';
    let result: IGrid[] = [];
    this.http.get<IGrid[]>(url).subscribe((grids) => {
      grids.forEach((grid) => result.push(grid))
    });

    return result;
    //  .pipe(
    //  catchError(this.handleError('getGrids', []))
    //);
  }

}

import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { strict } from 'assert';

import { ProjectExtents } from "../project/project-model"
import { DisplayMode } from "../project/project-displaymode-model"
import { TileData } from "../project/project-tiledata-model"

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class ProjectService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('ProjectService');
    this.baseUrl = baseUrl;
  }

  public getProjectExtents(projectUid: string): Observable<ProjectExtents> {
    let url = `${this.baseUrl}api/sitemodels/${projectUid}/extents`;
    console.log(url);
    return this.http.get<ProjectExtents>(url);
  }

  public getDisplayModes(): Observable<DisplayMode[]> {
    let url = `${this.baseUrl}api/tiles/modes`;
    console.log(url);
    return this.http.get<DisplayMode[]>(url);
  }

    public getTile(projectUid:string, mode: number, pixelsX: number, pixelsY:number, extents: ProjectExtents) {
    let url = `${this.baseUrl}api/tiles/${projectUid}?mode=${mode}&pixelsX=${pixelsX}&pixelsY=${pixelsY}&minX=${extents.minX}&minY=${extents.minY}&maxX=${extents.maxX}&maxY=${extents.maxY}`;
    console.log(url);
    return this.http.get<TileData>(url);
  }
}

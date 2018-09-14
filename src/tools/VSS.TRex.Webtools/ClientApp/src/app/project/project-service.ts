import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { strict } from 'assert';

import { ProjectExtents } from '../project/project-model';
import { DisplayMode } from '../project/project-displaymode-model';
import { TileData } from '../project/project-tiledata-model';
import { VolumeResult } from '../project/project-volume-model';

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

  private executeRequest<T>(label: string, url: string): Observable<T> {
    console.log(`{$label}: url=${url}`);
    return this.http.get<T>(url);
  }

  public getProjectExtents(projectUid: string): Observable<ProjectExtents> {
    return this.executeRequest<ProjectExtents>('getProjectExtents', `${this.baseUrl}api/sitemodels/${projectUid}/extents`);
  }

  public getDisplayModes(): Observable<DisplayMode[]> {
    return this.executeRequest<DisplayMode[]>('getDisplayModes', `${this.baseUrl}api/tiles/modes`);
  }

  public getTile(projectUid: string, mode: number, pixelsX: number, pixelsY: number, extents: ProjectExtents) {
    let url = `${this.baseUrl}api/tiles/${projectUid}?mode=${mode}&pixelsX=${pixelsX}&pixelsY=${pixelsY}&minX=${extents.minX}&minY=${extents.minY}&maxX=${extents.maxX}&maxY=${extents.maxY}`;
    return this.executeRequest<TileData>('getTile', url);
  }

  public getSimpleFullVolume(projectUid: string): Observable<VolumeResult> {
    return this.executeRequest<VolumeResult>('getSimpleFullVolume', `${this.baseUrl}api/volumes/${projectUid}`);
  }
}

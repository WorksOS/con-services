import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders,  } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { strict } from 'assert';

import { ProjectExtents, DesignDescriptor, SurveyedSurface, Design, Machine, ISiteModelMetadata, MachineEventType, MachineDesign } from '../project/project-model';
import { DisplayMode } from '../project/project-displaymode-model';
import { TileData } from '../project/project-tiledata-model';
import { VolumeResult } from '../project/project-volume-model';
import { CombinedFilter } from '../project/project-filter-model';

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

//    let headers = new HttpHeaders();
//    headers.append('Content-Type', 'application/json');
    //post data missing(here you pass email and password)

//    return this.http.post(url, body, headers);
    return this.http.post<T>(url, body);
  }

  private executeDeleteRequest<T>(label: string, url: string): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.delete<T>(url);
  }

  public getProjectExtents(projectUid: string): Observable<ProjectExtents> {
    return this.executeRequest<ProjectExtents>('getProjectExtents', `sitemodels/${projectUid}/extents`);
  }

  public getProjectDateRange(projectUid: string): Observable<any> {
    return this.executeRequest<any>('getProjectDateRange', `sitemodels/${projectUid}/daterange`);
  }

  public getDisplayModes(): Observable<DisplayMode[]> {
    return this.executeRequest<DisplayMode[]>('getDisplayModes', `tiles/modes`);
  }

  public getTile(projectUid: string, mode: number, pixelsX: number, pixelsY: number, extents: ProjectExtents) {
    let url = `tiles/${projectUid}?mode=${mode}&pixelsX=${pixelsX}&pixelsY=${pixelsY}&minX=${extents.minX}&minY=${extents.minY}&maxX=${extents.maxX}&maxY=${extents.maxY}`;
    return this.executeRequest<TileData>('getTile', url);
  }

  public getSimpleFullVolume(projectUid: string, filter: CombinedFilter): Observable<VolumeResult> {
    return this.executeRequest<VolumeResult>('getSimpleFullVolume', `volumes/${projectUid}?filter=${btoa(JSON.stringify(filter))}`);
  }

  public testJSONParameter(param: any): Observable<any> {
    var paramString : string = btoa(JSON.stringify(param));

    return this.executeRequest<CombinedFilter>('testJSONParameter', `sandbox/jsonparameter?param=${paramString}`);
  }

  public addSurveyedSurface(projectUid: string, descriptor: DesignDescriptor, asAtDate: Date, extents: ProjectExtents): Observable<SurveyedSurface> {

//    var newSurveyedSurface = new SurveyedSurface();

//    newSurveyedSurface.descriptor = descriptor;
//    newSurveyedSurface.asAtDate = asAtDate;
//    newSurveyedSurface.extents = extents;

//    return this.executePostRequest<string>
//      ('addSurveyedSurface',
//      `surveyedsurfaces/${projectUid}?newsurveyedsurface=${btoa(JSON.stringify(newSurveyedSurface))}`,
//      null);

    return this.executePostRequest<SurveyedSurface>
    ('addSurveyedSurface',
      `surveyedsurfaces/${projectUid}?fileName=${descriptor.fileName}&offset=${descriptor.offset}&asAtDate=${asAtDate.toISOString()}&minX=${extents.minX}&minY=${extents.minY}&maxX=${extents.maxX}&maxY=${extents.maxY}`,
      null);
  }

  public getSurveyedSurfaces(projectUid: string): Observable<SurveyedSurface[]> {
    return this.executeRequest<SurveyedSurface[]>('getSurveyedSurfaces', `surveyedsurfaces/${projectUid}`);
  }

  public deleteSurveyedSurface(projectUid: string, surveyedSurfaceId: string): Observable<any> {
    return this.executeDeleteRequest<any>('deleteSurveyedSurface', `surveyedsurfaces/${projectUid}/${surveyedSurfaceId}`);
  }

  public addDesign(projectUid: string, descriptor: DesignDescriptor, extents: ProjectExtents): Observable<DesignDescriptor> {
    return this.executePostRequest<DesignDescriptor>
      ('addDesign', `designs/${projectUid}?fileName=${descriptor.fileName}&minX=${extents.minX}&minY=${extents.minY}&maxX=${extents.maxX}&maxY=${extents.maxY}`, null);
  }

  public getDesigns(projectUid: string): Observable<Design[]> {
    return this.executeRequest<Design[]>('getDesigns', `designs/${projectUid}`);
  }

  public deleteDesign(projectUid: string, designId: string): Observable<any> {
    return this.executeDeleteRequest<any>('deleteDesign', `designs/${projectUid}/${designId}`);
  }

  public getMachineDesigns(projectUid: string): Observable<MachineDesign[]> {
    return this.executeRequest<MachineDesign[]>('getMachineDesigns', `sitemodels/${projectUid}/machinedesigns`);
  }

  public getMachines(projectUid: string): Observable<Machine[]> {
    return this.executeRequest<Machine[]>('getMachines', `machines/${projectUid}`);
  }

  public getExistenceMapSubGridCount(projectUid: string): Observable<number> {
    return this.executeRequest<number>('getExistenceMapSubGridCount', `sitemodels/${projectUid}/existencemap/subgridcount`);
  }

  public getAllProjectMetadata(): Observable<ISiteModelMetadata[]> {
    return this.executeRequest<ISiteModelMetadata[]>('getAllProjectMetadata', `sitemodels/metadata`);
  }

  public getMachineEventTypes(): Observable<MachineEventType[]> {
    return this.executeRequest<MachineEventType[]>('getMachineEventTypes', `events/types`);
  }

  public getMachineEvents(projectUid: string, machineUid: string, eventType: number,
    startDate: Date, endDate: Date, maxEventsToReturn: number): Observable<string[]> {
    return this.executeRequest<string[]>('getMachineEvents', `events/text/${projectUid}/${machineUid}/${eventType}?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&maxEventsToReturn=${maxEventsToReturn}`);
  }

  public switchToMutable(): Observable<string> {
    return this.executePutRequest<string>('switchToMutable', 'switchablegrid/mutable');
  }

  public switchToImmutable(): Observable<string> {
    return this.executePutRequest<string>('switchToImmutable', 'switchablegrid/immutable');
  }
}

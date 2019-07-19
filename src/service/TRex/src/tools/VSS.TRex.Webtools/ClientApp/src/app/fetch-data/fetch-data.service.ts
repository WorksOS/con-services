import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { DataRequestType, DisplayModeType, OverrideParameters, OverrideRange } from './fetch-data-model';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class FetchDataService {
  private handleError: HandleError;
  private baseUrl: string;

  private KM_HR_TO_CM_SEC: number = 27.77777778; //1.0 / 3600 * 100000;

  private globalOverrideDefaults: OverrideParameters = new OverrideParameters(
      false, 700, new OverrideRange(80, 130),
      false, 700, new OverrideRange(80, 130),
      false, new OverrideRange(6, 6),
      false, new OverrideRange(650, 1750),
    new OverrideRange(5 * this.KM_HR_TO_CM_SEC, 10 * this.KM_HR_TO_CM_SEC)); 


  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('FetchDataService');
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

  public getDataRequestTypes(): Observable<DataRequestType[]> {
    return this.executeRequest<DataRequestType[]>("getDataRequestTypes", `productiondata/requesttypes`);
  }

  public getProductionData(projectUid: string, requestType: number, designUid: string, designOffset: number, overrides:OverrideParameters): Observable<string> {
    let requestTypeString: string = ""; 
    switch (<DisplayModeType>requestType)
    {
      case DisplayModeType.CCV:
        requestTypeString = "cmvdetails";
        break;
      case DisplayModeType.CCVPercentSummary:
        requestTypeString = "cmvsummary";
        break;
      case DisplayModeType.CMVChange:
        requestTypeString = "cmvchange";
        break;
      case DisplayModeType.MDPPercentSummary:
        requestTypeString = "mdpsummary";
        break;
      case DisplayModeType.PassCount:
        requestTypeString = "passcountdetails";
        break;
      case DisplayModeType.PassCountSummary:
        requestTypeString = "passcountsummary";
        break;
      case DisplayModeType.CCASummary:
        requestTypeString = "ccasummary";
        break;
      case DisplayModeType.TemperatureDetail:
        requestTypeString = "temeraturedetails";
        break;
      case DisplayModeType.TemperatureSummary:
        requestTypeString = "temeraturesummary";
        break;
      case DisplayModeType.TargetSpeedSummary:
        requestTypeString = "machinespeedsummary";
        break;
      case DisplayModeType.CutFill:
        requestTypeString = "cutfillstatistics";
        break;
      case DisplayModeType.ElevationRange:
        requestTypeString = "elevationrange";
    };

    if (requestTypeString === "")
      return undefined;

    let url = `productiondata/${requestTypeString}/${projectUid}`;
    if (<DisplayModeType>requestType == DisplayModeType.CutFill) {
        url += `?cutFillDesignUid=${designUid}&cutFillOffset=${designOffset}`;
    }
    return this.executePostRequest<string>("getProductionData", url, overrides);
  }

  public getDefaultOverrides() {
      //TODO: may need to make a copy so changes in 'fetch' page don't change the defaults
      return this.globalOverrideDefaults;
  }

}

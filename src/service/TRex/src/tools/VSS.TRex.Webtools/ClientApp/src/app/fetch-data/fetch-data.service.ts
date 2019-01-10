import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';
import { DataRequestType, DisplayModeType } from './fetch-data-model';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class FetchDataService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('FetchDataService');
    this.baseUrl = baseUrl;
  }

  private executeRequest<T>(label: string, url: string): Observable<T> {
    url = `${this.baseUrl}api/${url}`;
    console.log(`${label}: url=${url}`);
    return this.http.get<T>(url);
  }

  public getDataRequestTypes(): Observable<DataRequestType[]> {
    return this.executeRequest<DataRequestType[]>("getDataRequestTypes", `productiondata/requesttypes`);
  }

  public getProductionData(projectUid: string, requestType: number): Observable<string> {
    let requestTypeString: string = ""; 
    switch (<DisplayModeType>requestType)
    {
      case DisplayModeType.CCV:
        requestTypeString = "cmvdetails";
        break;
      case DisplayModeType.CCVSummary:
        requestTypeString = "cmvsummary";
        break;
      case DisplayModeType.CMVChange:
        requestTypeString = "cmvchange";
        break;
      case DisplayModeType.MDPSummary:
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
    };

    if (requestTypeString === "")
      return undefined;

    let url = `productiondata/${requestTypeString}/${projectUid}`;
    return this.executeRequest<string>("getProductionData", url);
  }

}

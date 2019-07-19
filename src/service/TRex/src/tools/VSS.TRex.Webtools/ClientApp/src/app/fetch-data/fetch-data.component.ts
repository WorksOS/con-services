import { Component } from '@angular/core';
import { FetchDataService } from './fetch-data.service';
import { DataRequestType, OverrideParameters, OverrideRange } from './fetch-data-model';


@Component({
  selector: 'fetch-data',
  templateUrl: './fetch-data.component.html',
  providers: [FetchDataService],
  styleUrls: ['./fetch-data.component.css']
})
export class FetchDataComponent {

  private KM_HR_TO_CM_SEC: number = 27.77777778; //1.0 / 3600 * 100000;

  public allDataRequestTypes: DataRequestType[] = [];
  public dataRequestType: DataRequestType = new DataRequestType();
  public requestType: number = 1;

  public dataResult: any;

  private projectUid: string = localStorage.getItem("projectUid");
  private designUid: string = localStorage.getItem("designUid");
  private designOffset: number = parseFloat(localStorage.getItem("designOffset"));
  private overrides: OverrideParameters = new OverrideParameters(
      false, 700, new OverrideRange(80, 130),
      false, 700, new OverrideRange(80, 130),
      false, new OverrideRange(6, 6),
      false, new OverrideRange(650, 1750),
      new OverrideRange(5 * this.KM_HR_TO_CM_SEC, 10 * this.KM_HR_TO_CM_SEC));
  
  constructor(
    private fetchDataService: FetchDataService
  ) { }

  ngOnInit() {
    var self = this;

    this.fetchDataService.getDataRequestTypes().subscribe((types) => {
      types.forEach(type => self.allDataRequestTypes.push(type));
      self.dataRequestType = self.allDataRequestTypes[0];
    });
  }

  public noProjectSelected(): boolean {
    return this.projectUid === "undefined";
  }

  public dataRequestTypeChanged(event: any): void {
    this.requestType = this.dataRequestType.item1;
  }

  public getProductionData(): void {
    var self = this;

    this.fetchDataService.getProductionData(this.projectUid, this.requestType, this.designUid, this.designOffset, this.overrides).subscribe(data => {
      self.dataResult = data;
    });
  }
}

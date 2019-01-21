import { Component } from '@angular/core';
import { FetchDataService } from './fetch-data.service';
import { DataRequestType } from './fetch-data-model';


@Component({
  selector: 'fetch-data',
  templateUrl: './fetch-data.component.html',
  providers: [FetchDataService],
  styleUrls: ['./fetch-data.component.css']
})
export class FetchDataComponent {

  public allDataRequestTypes: DataRequestType[] = [];
  public dataRequestType: DataRequestType = new DataRequestType();
  public requestType: number = 1;

  public dataResult: any;

  private projectUid: string = localStorage.getItem("projectUid");
  
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

    this.fetchDataService.getProductionData(this.projectUid, this.requestType).subscribe(data => {
      self.dataResult = data;
    });
  }
}

import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
  selector: 'gridStatus',
  templateUrl: './gridStatus.component.html'
})
export class gridStatus {
  public status: string = "Unknown";
  private baseUrl: string;
  private http: Http;

  
  constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
    this.getStatus();
  }

  public getStatus() {
    console.log("Getting Status")
    this.http.get(this.baseUrl + 'api/gridStatus').subscribe(result => {
      this.status = result.text();
    }, error => console.error(error));
  }

}


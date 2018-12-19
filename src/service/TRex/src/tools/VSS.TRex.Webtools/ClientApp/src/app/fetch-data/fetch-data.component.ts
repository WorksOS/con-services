import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { FetchDataService } from './fetch-data.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'fetch-data',
  templateUrl: './fetch-data.component.html',
  providers: [FetchDataService],
  styleUrls: ['./fetch-data.component.css']
})

export class FetchDataComponent {

  constructor(private fetchDataService: FetchDataService ) {}
}

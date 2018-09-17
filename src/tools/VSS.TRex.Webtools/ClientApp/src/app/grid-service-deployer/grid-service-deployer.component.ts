import { Component } from '@angular/core';
import { GridServiceDeployerService } from './grid-service-deployer.service';


@Component({
  selector: 'grid-service-deployer',
  templateUrl: './grid-service-deployer.component.html',
  providers: [GridServiceDeployerService]
})
export class GridServiceDeployerComponent {

  constructor(
    private gridServiceDeployerService: GridServiceDeployerService
  ) { }

  public deployTagFileQueue() {
    console.log(`Deploying tagfile buffer queue`)
    this.gridServiceDeployerService.deployTagFileBufferQueueService();
  }
}


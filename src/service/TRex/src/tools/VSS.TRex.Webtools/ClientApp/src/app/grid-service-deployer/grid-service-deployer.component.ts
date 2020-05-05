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
    console.log(`Deploying tagfile buffer queue service`);
    this.gridServiceDeployerService.deployTagFileBufferQueueService();
  }

  public deployMutableSegmentRetirementQueue() {
    console.log(`Deploying mutable segment retirement queue service`);
    this.gridServiceDeployerService.deployMutableSegmentRetirementQueueService();
    }

  public deploySiteModelChangeProcessorService() {
    console.log(`Deploying site model change processor service`);
    this.gridServiceDeployerService.deployMutableSegmentRetirementQueueService();
  }

  public deployAllServices() {
    this.deployTagFileQueue();
    this.deployMutableSegmentRetirementQueue();
    this.deploySiteModelChangeProcessorService();
  }
}


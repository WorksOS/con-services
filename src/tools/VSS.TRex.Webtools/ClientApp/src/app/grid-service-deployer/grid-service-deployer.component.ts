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
    console.log(`Deploying tagfile buffer queue`);
    this.gridServiceDeployerService.deployTagFileBufferQueueService();
  }

  public deployMutableSegmentRetirementQueue() {
    console.log(`Deploying mutable segment retirement queue`);
    this.gridServiceDeployerService.deployMutableSegmentRetirementQueueService();
  }

  public deployImmutableSegmentRetirementQueue() {
    console.log(`Deploying immutable segment retirement queue`);
    this.gridServiceDeployerService.deployImmutableSegmentRetirementQueueService();
  }
}


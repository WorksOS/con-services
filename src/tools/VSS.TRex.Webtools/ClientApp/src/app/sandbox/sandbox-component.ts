import { Component } from '@angular/core';
import { ProjectExtents } from './sandbox-model';
import { SandboxService } from './sandbox-service';

@Component({
  selector: 'sandbox',
  templateUrl: './sandbox-component.html',
  providers: [SandboxService]
})

export class SandboxComponent {

 // title = 'sandbox';

  constructor(
    private sandboxService: SandboxService
  ) { }

  public projectUid: string;

  public projectExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);

  public getProjectExtents(): void {

    this.sandboxService.getProjectExtents(this.projectUid).subscribe(extent => {
      this.projectExtents = new ProjectExtents(extent.minX, extent.minY, extent.maxX, extent.maxY)
    });
  }
}


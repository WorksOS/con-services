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
  public mode: number = 0;
  public pixelsX: number = 100;
  public pixelsY: number = 100;

  public base64EncodedTile:string = "";
  
  public projectExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);

  public getProjectExtents(): void {
    this.sandboxService.getProjectExtents(this.projectUid).subscribe(extent => {
      this.projectExtents = new ProjectExtents(extent.minX, extent.minY, extent.maxX, extent.maxY)
    });
  }
  //00000000-0000-0000-0000-000000000011
  public getTile(): void {  
    this.sandboxService.getTile(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.projectExtents).subscribe(tile => {
      this.base64EncodedTile = "data:image/bmp;base64,".concat(tile.tileData);
    });
  }
}


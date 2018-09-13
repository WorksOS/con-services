import { Component } from '@angular/core';
import { ProjectExtents } from './project-model';
import { ProjectService } from './project-service';
import { DisplayMode } from './project-displaymode-model';
import { VolumeResult } from "../project/project-volume-model"

@Component({
  selector: 'project',
  templateUrl: './project-component.html',
  providers: [ProjectService]
})

export class ProjectComponent {
  public projectUid: string;
  public mode: number = 0;
  public pixelsX: number = 500;
  public pixelsY: number = 500;

  public base64EncodedTile: string = '';

  public displayModes: DisplayMode[] = [];
  public displayMode: DisplayMode = new DisplayMode();

  public projectExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
  public tileExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);

  public projectVolume: VolumeResult = new VolumeResult(0, 0, 0, 0, 0);

  constructor(
    private projectService: ProjectService
  ) { }

  ngOnInit() { 
    this.projectService.getDisplayModes().subscribe((modes) => {
      modes.forEach(mode => this.displayModes.push(mode));
      this.displayMode = this.displayModes[0];
    });
  }

  public selectProject(): void {
    this.getProjectExtents();
  }

  public setProjectToZero(): void {
    this.projectUid = "00000000-0000-0000-0000-000000000000";
  }

  public getProjectExtents(): void {
    this.projectService.getProjectExtents(this.projectUid).subscribe(extent => {
      this.projectExtents = new ProjectExtents(extent.minX, extent.minY, extent.maxX, extent.maxY);
      this.zoomAll();
    });
  }

  public displayModeChanged(event : any): void {
    this.mode = this.displayMode.item1;
    this.getTile();
  }

  //00000000-0000-0000-0000-000000000011

  public getTile(): void {
    // Make sure the displayed tile extents is updated
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.projectService.getTile(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents)
      .subscribe(tile => this.base64EncodedTile = 'data:image/png;base64,' + tile.tileData);
  }

  public zoomAll(): void {
    this.tileExtents = new ProjectExtents(this.projectExtents.minX, this.projectExtents.minY, this.projectExtents.maxX, this.projectExtents.maxY);
    this.getTile();
  }

  public zoomIn(): void {
    this.tileExtents.shrink(0.2);
    this.getTile();
  }

  public zoomOut(): void {
    this.tileExtents.expand(0.2);
    this.getTile();
  }

  public panLeft(): void {
    this.tileExtents.pan(-0.2, 0.0);
    this.getTile();
  }

  public panRight(): void {
    this.tileExtents.pan(0.2, 0.0);
    this.getTile();
  }

  public panUp(): void {
    this.tileExtents.pan(0, 0.2);
    this.getTile();
  }

  public panDown(): void {
    this.tileExtents.pan(0, -0.2);
    this.getTile();
  }

  public getSimpleFullVolume() : void {
    this.projectService.getSimpleFullVolume(this.projectUid).subscribe(volume =>
      this.projectVolume = new VolumeResult(volume.cut, volume.cutArea, volume.fillArea, volume.fillArea, volume.totalCoverageArea));
  }

}


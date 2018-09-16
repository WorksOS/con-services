import { Component } from '@angular/core';
import { ProjectExtents } from './project-model';
import { ProjectService } from './project-service';
import { DisplayMode } from './project-displaymode-model';
import { VolumeResult } from '../project/project-volume-model';
import { CombinedFilter, SpatialFilter, AttributeFilter, FencePoint} from '../project/project-filter-model';

@Component({
  selector: 'project',
  templateUrl: './project-component.html',
  providers: [ProjectService]
//  styleUrls: ['./project.component.less']
})

export class ProjectComponent {
  private zoomFactor: number = 0.2;

  public projectUid: string; 
  public mode: number = 0;
  public pixelsX: number = 850;
  public pixelsY: number = 500;

  public base64EncodedTile: string = '';

  public displayModes: DisplayMode[] = [];
  public displayMode: DisplayMode = new DisplayMode();

  public projectExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
  public tileExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);

  public projectVolume: VolumeResult = new VolumeResult(0, 0, 0, 0, 0);

  public mousePixelLocation : string;
  public mouseWorldLocation: string;

  private mouseWorldX: number = 0;
  private mouseWorldY: number = 0;

  public timerStartTime: number = performance.now();
  public timerEndTime: number = performance.now();
  public timerTotalTime: number = 0;

  public applyToViewOnly: boolean = false;
    
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

  public getTile(): void {
    // Make sure the displayed tile extents is updated
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.projectService.getTile(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents)
      .subscribe(tile => {
        this.base64EncodedTile = 'data:image/png;base64,' + tile.tileData;
        this.updateTimerCompletionTime();      
      });
  }

  public zoomAll(): void {
  //  this.tileExtents = new ProjectExtents(this.projectExtents.minX, this.projectExtents.minY, this.projectExtents.maxX, this.projectExtents.maxY);

    // Square up the tileExtents to match the aspect ratio between the displayed image and the requested world area

    let tileExtents = new ProjectExtents(this.projectExtents.minX, this.projectExtents.minY, this.projectExtents.maxX, this.projectExtents.maxY);

    if ((this.projectExtents.sizeX() / this.projectExtents.sizeY()) > (this.pixelsX / this.pixelsY)) {
      // The project extents are 'wider' than the display, make the tile extent taller to compensate
      let ratioFraction = (this.projectExtents.sizeX() / this.projectExtents.sizeY()) / (this.pixelsX / this.pixelsY);
      tileExtents.expand(0, ratioFraction - 1);
    } else {
      // The project extents are 'shorter' than the display, , make the tile extent wider to compensate
      let ratioFraction = (this.projectExtents.sizeY() / this.projectExtents.sizeX()) / (this.pixelsY / this.pixelsX) ;
      tileExtents.expand(ratioFraction - 1, 0);
    }

    // Assign modified extents into bound model 
    this.tileExtents = tileExtents;
    this.getTile();
  }

  public zoomIn(): void {
    this.tileExtents.shrink(this.zoomFactor, this.zoomFactor);
    this.getTile();
  }

  public zoomOut(): void {
    this.tileExtents.expand(this.zoomFactor, this.zoomFactor);
    this.getTile();
  }

  public panLeft(): void {
    this.tileExtents.panByFactor(-this.zoomFactor, 0.0);
    this.getTile();
  }

  public panRight(): void {
    this.tileExtents.panByFactor(this.zoomFactor, 0.0);
    this.getTile();
  }

  public panUp(): void {
    this.tileExtents.panByFactor(0, this.zoomFactor);
    this.getTile();
  }

  public panDown(): void {
    this.tileExtents.panByFactor(0, -this.zoomFactor);
    this.getTile();
  }

  public getSimpleFullVolume(): void {
    var filter = new CombinedFilter();

    if (this.applyToViewOnly) {
      filter.spatialFilter.coordsAreGrid = true;

      filter.spatialFilter.isSpatial = true;
      filter.spatialFilter.Fence.isRectangle = true;

      filter.spatialFilter.Fence.Points = [];
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.minX, this.tileExtents.minY));
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.minX, this.tileExtents.maxY));
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.maxX, this.tileExtents.maxY));
      filter.spatialFilter.Fence.Points.push(new FencePoint(this.tileExtents.maxX, this.tileExtents.minY));
    }

    this.projectService.getSimpleFullVolume(this.projectUid, filter).subscribe(volume =>
      this.projectVolume = new VolumeResult(volume.cut, volume.cutArea, volume.fillArea, volume.fillArea, volume.totalCoverageArea));
  }

  private updateMouseLocationDetails(offsetX : number, offsetY: number): void {
    let localX = offsetX;
    let localY = this.pixelsY - offsetY;

    this.mouseWorldX = this.tileExtents.minX + offsetX * (this.tileExtents.sizeX() / this.pixelsX);
    this.mouseWorldY = this.tileExtents.minY + (this.pixelsY - offsetY) * (this.tileExtents.sizeY() / this.pixelsY);

    this.mousePixelLocation = `${localX}, ${localY}`;
    this.mouseWorldLocation = `${this.mouseWorldX.toFixed(3)}, ${this.mouseWorldY.toFixed(3)}`;
  }

  public onMouseOver(event: any): void {
    this.updateMouseLocationDetails(event.offsetX, event.offsetY);
  }

  public onMouseMove(event: any): void {
    this.updateMouseLocationDetails(event.offsetX, event.offsetY);
  }

  public onMouseWheel(event: any): void {
    let panDeltaX = this.zoomFactor * (this.mouseWorldX - this.tileExtents.centerX());
    let panDeltaY = this.zoomFactor * (this.mouseWorldY - this.tileExtents.centerY());

    if (event.deltaY < 0) {
      // Zooming in 
      this.tileExtents.panByDelta(panDeltaX, panDeltaY);
      this.zoomIn();
    } else if (event.deltaY > 0) {
      // Zooming out
      this.tileExtents.panByDelta(-panDeltaX, -panDeltaY);
      this.zoomOut();
    }
  }

  public timeSomething(doSomething: () => void): void {
    this.timerStartTime = performance.now();
    doSomething();
    this.updateTimerCompletionTime();
  }

  public performNTimes(doSomething: () => void, count: number): void {
    for (var i = 0; i < 10; i++) {
      doSomething();
    }
  }

  public getTile10x(): void {
    this.timeSomething(() => this.performNTimes(() => this.getTile(), 10));
  }

  private updateTimerCompletionTime() : void {
    this.timerEndTime = performance.now();
    this.timerTotalTime = this.timerEndTime - this.timerStartTime;
  }

  public testJSONParameter(): void {
    var filter: CombinedFilter = new CombinedFilter();
    filter.spatialFilter = new SpatialFilter();
    filter.attributeFilter = new AttributeFilter();

    this.projectService.testJSONParameter(filter).subscribe(x => x);
  }
}


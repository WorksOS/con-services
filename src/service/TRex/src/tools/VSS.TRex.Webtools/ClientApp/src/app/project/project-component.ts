import { Component } from '@angular/core';
import { ProjectExtents, DesignDescriptor, SurveyedSurface, DesignSurface, Alignment, Machine, ISiteModelMetadata, MachineEventType, MachineDesign, SiteProofingRun, XYZS } from './project-model';
import { ProjectService } from './project-service';
import { DisplayMode } from './project-displaymode-model';
import { VolumeResult } from '../project/project-volume-model';
import { CombinedFilter, SpatialFilter, AttributeFilter, FencePoint } from '../project/project-filter-model';
import { CellDatumResult } from "./project-model";
import { SummaryType } from './project-summarytype-model';
import { OverrideParameters, OverrideRange } from '../fetch-data/fetch-data-model';
import { FetchDataService } from '../fetch-data/fetch-data.service';

@Component({
  selector: 'project',
  templateUrl: './project-component.html',
  providers: [ProjectService, FetchDataService]
  //  styleUrls: ['./project.component.less']
})
export class ProjectComponent {
  private zoomFactor: number = 0.2;

  public currentGridName: string;

  public projectUid: string;
  public mode: number = 0;
  public pixelsX: number = 1000;
  public pixelsY: number = 500;

  public base64EncodedTile: string = '';

  public displayModes: DisplayMode[] = [];
  public displayMode: DisplayMode = new DisplayMode();

  public summaryTypes: SummaryType[] = [];

  public eventTypes: MachineEventType[] = [];
  public eventType: MachineEventType = new MachineEventType();

  public projectExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
  public tileExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);

  public projectStartDate: Date = new Date();
  public projectEndDate: Date = new Date();

  public projectVolume: VolumeResult = new VolumeResult(0, 0, 0, 0, 0);

  public mousePixelLocation: string;
  public mouseWorldLocation: string;

  private mousePixelX: number = 0;
  private mousePixelY: number = 0;

  private prevMousePixelX: number = -1;
  private prevMousePixelY: number = -1;

  private mouseWorldX: number = 0;
  private mouseWorldY: number = 0;

  public timerStartTime: number = performance.now();
  public timerEndTime: number = performance.now();
  public timerTotalTime: number = 0;

  public applyToViewOnly: boolean = false;

  public surveyedSurfaceFileName: string = "";
  public surveyedSurfaceAsAtDate: Date = new Date();

  public newSurveyedSurfaceGuid: string = "";
  public surveyedSurfaces: SurveyedSurface[] = [];

  public alignments: Alignment[] = [];
  public alignmentFileName: string = "";
  public newAlignmentGuid: string = "";

  public designs: DesignSurface[] = [];
  public designFileName: string = "";
  public designOffset: number = 0.0;
  public designUid: string = "";

  public newDesignGuid: string = "";
  public machineDesigns: MachineDesign[] = [];
  public siteProofingRuns: SiteProofingRun[] = [];

  public machines: Machine[] = [];
  public machine: Machine = new Machine();

  public existenceMapSubGridCount: number = 0;

  private kMaxTestElev: number = 100000.0;
  private kMinTestElev: number = -100000.0;

  public machineColumns: string[] =
    [
      "id",
      "internalSiteModelMachineIndex",
      "name",
      "machineType",
      "deviceType",
      "machineHardwareID",
      "isJohnDoeMachine",
      "lastKnownX",
      "lastKnownY",
      "lastKnownPositionTimeStamp",
      "lastKnownDesignName",
      "lastKnownLayerId"
    ];

  public machineColumnNames: string[] =
    [
      "ID",
      "Index", // "internalSiteModelMachineIndex"
      "Name",
      "Type",
      "Device",
      "Hardware ID",
      "John Doe",
      "Last Known X",
      "Last Known Y",
      "Last Known Date",
      "Last Known Design",
      "Last Known Layer"
    ];

  public projectMetadata: ISiteModelMetadata;

  public allProjectsMetadata: ISiteModelMetadata[] = [];

  public machineEvents: string[] = [];
  public machineEventsStartDate: Date = new Date(1980, 1, 1, 0, 0, 0, 0);
  public machineEventsEndDate: Date = new Date(2100, 1, 1, 0, 0, 0, 0);
  public maxMachineEventsToReturn: number = 100;

  public profilePath: string = "M0 0 L200 500 L400 0 L600 500 L800 0 L1000 500";

  public compositeElevationProfilePath_LastElev: string = "";
  public compositeElevationProfilePath_FirstElev: string = "";
  public compositeElevationProfilePath_LowestElev: string = "";
  public compositeElevationProfilePath_HighestElev: string = "";
  public compositeElevationProfilePath_LastCompositeElev: string = "";
  public compositeElevationProfilePath_FirstCompositeElev: string = "";
  public compositeElevationProfilePath_LowestCompositeElev: string = "";
  public compositeElevationProfilePath_HighestCompositeElev: string = "";

  public _compositeElevationProfilePath_LastElev: string = "";
  public _compositeElevationProfilePath_FirstElev: string = "";
  public _compositeElevationProfilePath_LowestElev: string = "";
  public _compositeElevationProfilePath_HighestElev: string = "";
  public _compositeElevationProfilePath_LastCompositeElev: string = "";
  public _compositeElevationProfilePath_FirstCompositeElev: string = "";
  public _compositeElevationProfilePath_LowestCompositeElev: string = "";
  public _compositeElevationProfilePath_HighestCompositeElev: string = "";

  public showLastElevationProfile: boolean = true;
  public showFirstElevationProfile: boolean = true;
  public showLowestElevationProfile: boolean = true;
  public showHighestElevationProfile: boolean = true;
  public showLastCompositeElevationProfile: boolean = true;
  public showFirstCompositeElevationProfile: boolean = true;
  public showLowestCompositeElevationProfile: boolean = true;
  public showHighestCompositeElevationProfile: boolean = true;

  public userProfilePath: string = "";
  public userProfilePoint1SVG_CX: number = 0;
  public userProfilePoint1SVG_CY: number = 0;
  public userProfilePoint2SVG_CX: number = 0;
  public userProfilePoint2SVG_CY: number = 0;
  public userProfilePoint1SVG_WorldX: number = 0;
  public userProfilePoint1SVG_WorldY: number = 0;
  public userProfilePoint2SVG_WorldX: number = 0;
  public userProfilePoint2SVG_WorldY: number = 0;

  public numPointInProfile: number = 0;

  public updateFirstPointLocation: boolean = false;
  public updateSecondPointLocation: boolean = false;
  public updateFirstPointLocationSV: boolean = false;
  public updateSecondPointLocationSV: boolean = false;

  public firstPointX: number = 0.0;
  public firstPointY: number = 0.0;

  public secondPointX: number = 0.0;
  public secondPointY: number = 0.0;

  public svFirstPointX: number = 0.0;
  public svFirstPointY: number = 0.0;

  public svSecondPointX: number = 0.0;
  public svSecondPointY: number = 0.0;

  public profileExtents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
  public mouseProfileWorldStation: number = 0.0;
  public mouseProfileWorldZ: number = 0.0;

  public mouseProfilePixelLocation: string = '';
  public mouseProfileWorldLocation: string = '';

  public profileValue: string = '';
  public profileValue2: string = '';
  private profilePoints: any[];
  private profileCanvasHeight: number = 500.0;
  private profileCanvasWidth: number = 1000.0;
  private overrides:OverrideParameters;

  public cellDatum: string = "";
  private showCellDatum: boolean = false;

  constructor(
    private projectService: ProjectService,
    private fetchDataService: FetchDataService
  ) { }

  ngOnInit() {
    this.projectService.getDisplayModes().subscribe((modes) => {
      modes.forEach(mode => this.displayModes.push(mode));
      this.displayMode = this.displayModes[0];
    });

    this.projectService.getSummaryTypes().subscribe((summaryTypes) => {
        summaryTypes.forEach(summaryType => this.summaryTypes.push(summaryType));
    });

    this.projectService.getMachineEventTypes().subscribe((types) => {
      types.forEach(type => this.eventTypes.push(type));
      this.eventType = this.eventTypes[0];
    });

    if ("overrides" in localStorage) {
        this.overrides = JSON.parse(localStorage.getItem("overrides"));
    } else {
        this.overrides = this.fetchDataService.getDefaultOverrides();
    }

    this.getAllProjectMetadata();

    this.switchToImmutable();
  }

  public selectProject(): void {
    this.getProjectExtents();
    this.getExistenceMapSubGridCount();
    this.getSurveyedSurfaces();
    this.getDesignSurfaces();
    this.getAlignments();
    this.getMachines();
    this.getMachineDesigns();
    this.getSiteProofingRuns();

    // Sleep for half a second to allow the project extents result to come back, then zoom all
    setTimeout(() => this.zoomAll(), 250);
  }

  public setProjectToZero(): void {
    this.projectUid = "00000000-0000-0000-0000-000000000000";
    localStorage.setItem("projectUid", undefined);
    localStorage.setItem("designUid", undefined);
    localStorage.setItem("designOffset", undefined);
    this.overrides = this.fetchDataService.getDefaultOverrides();
    localStorage.setItem("overrides", JSON.stringify(this.overrides));
  }

  public getProjectExtents(): void {
    this.projectService.getProjectExtents(this.projectUid).subscribe(extent => {
      this.projectExtents = new ProjectExtents(extent.minX, extent.minY, extent.maxX, extent.maxY);
    });

    this.projectService.getProjectDateRange(this.projectUid).subscribe(dateRange => {
      this.projectStartDate = dateRange.item1;
      this.projectEndDate = dateRange.item2;
    });
  }

  public displayModeChanged(event: any): void {
    this.mode = this.displayMode.item1;
    this.getTileXTimes(1);
  }

  public async getTileAsync(projectUid: string,
    mode: number,
    pixelsX: number,
    pixelsY: number,
    tileExtents: ProjectExtents,
    designUid: string,
    designOffset: number): Promise<string> {

    var vm = this;
    return new Promise<string>((resolve) => this.projectService.getTile(projectUid, mode, pixelsX, pixelsY, tileExtents, designUid, designOffset)
      .subscribe(tile => {
        vm.base64EncodedTile = 'data:image/png;base64,' + tile.tileData;
        resolve();
      }
      ));
  }

  public performNTimesSync(doSomething: (numRemaining: number) => Promise<any>, count: number): void {
    let result: Promise<any> = doSomething(count);
    result.then(() => {
      if (count > 0) {
        doSomething(count - 1);
      }
    });
  }

  public testAsync() {
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.performNTimesSync(() => this.getTileAsync(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents, this.designUid, this.designOffset), 10);
  }

  public getTile(): void {
    // If there is no project bail...
    if (!this.projectUid)
      return;

    // Make sure the displayed tile extents is updated
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.projectService.getTile(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents, this.designUid, this.designOffset)
      .subscribe(tile => {
        this.base64EncodedTile = 'data:image/png;base64,' + tile.tileData;
        this.updateTimerCompletionTime();
      });
  }

  public zoomAllXTimes(xTimes: number): void {
    this.timeSomething(() => this.performNTimes(() => this.zoomAll(), xTimes));
  }

  public zoomAll(): void {
    // Ensure our project bounds are up to date
    this.getProjectExtents();

    // Square up the tileExtents to match the aspect ratio between the displayed image and the requested world area

    let tileExtents = new ProjectExtents(this.projectExtents.minX, this.projectExtents.minY, this.projectExtents.maxX, this.projectExtents.maxY);

    if ((this.projectExtents.sizeX() / this.projectExtents.sizeY()) > (this.pixelsX / this.pixelsY)) {
      // The project extents are 'wider' than the display, make the tile extent taller to compensate
      let ratioFraction = (this.projectExtents.sizeX() / this.projectExtents.sizeY()) / (this.pixelsX / this.pixelsY);
      tileExtents.expand(0, ratioFraction - 1);
    } else {
      // The project extents are 'shorter' than the display, , make the tile extent wider to compensate
      let ratioFraction = (this.projectExtents.sizeY() / this.projectExtents.sizeX()) / (this.pixelsY / this.pixelsX);
      tileExtents.expand(ratioFraction - 1, 0);
    }

    // Assign modified extents into bound model 
    this.tileExtents = tileExtents;
    this.getTile();
  }

  public zoomIn(): void {
    this.tileExtents.shrink(this.zoomFactor, this.zoomFactor);
    this.UpdateProfileLineOnMap();
    this.getTileXTimes(1);
  }

  public zoomOut(): void {
    this.tileExtents.expand(this.zoomFactor, this.zoomFactor);
    this.UpdateProfileLineOnMap();
    this.getTileXTimes(1);
  }

  public panLeft(): void {
    this.tileExtents.panByFactor(-this.zoomFactor, 0.0);
    this.UpdateProfileLineOnMap();
    this.getTileXTimes(1);
  }

  public panRight(): void {
    this.tileExtents.panByFactor(this.zoomFactor, 0.0);
    this.UpdateProfileLineOnMap();
    this.getTileXTimes(1);
  }

  public panUp(): void {
    this.tileExtents.panByFactor(0, this.zoomFactor);
    this.UpdateProfileLineOnMap();
    this.getTileXTimes(1);
  }

  public panDown(): void {
    this.tileExtents.panByFactor(0, -this.zoomFactor);
    this.UpdateProfileLineOnMap();
    this.getTileXTimes(1);
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

  public toggleCellDatum(): void {
    this.showCellDatum = !this.showCellDatum;
  }

  public UpdateProfileLineOnMap(): void {
    this.userProfilePoint1SVG_CX = (this.userProfilePoint1SVG_WorldX - this.tileExtents.minX) / (this.tileExtents.sizeX() / this.pixelsX);
    this.userProfilePoint1SVG_CY = this.pixelsY - (this.userProfilePoint1SVG_WorldY - this.tileExtents.minY) / (this.tileExtents.sizeY() / this.pixelsY);
    this.userProfilePoint2SVG_CX = (this.userProfilePoint2SVG_WorldX - this.tileExtents.minX) / (this.tileExtents.sizeX() / this.pixelsX);
    this.userProfilePoint2SVG_CY = this.pixelsY - (this.userProfilePoint2SVG_WorldY - this.tileExtents.minY) / (this.tileExtents.sizeY() / this.pixelsY);

    this.userProfilePath = `M${this.userProfilePoint1SVG_CX},${this.userProfilePoint1SVG_CY} L${this.userProfilePoint2SVG_CX},${this.userProfilePoint2SVG_CY}`;
  }

  private updateMouseLocationDetails(offsetX: number, offsetY: number): void {
    this.mousePixelX = offsetX;
    this.mousePixelY = this.pixelsY - offsetY;

    this.mouseWorldX = this.tileExtents.minX + offsetX * (this.tileExtents.sizeX() / this.pixelsX);
    this.mouseWorldY = this.tileExtents.minY + (this.pixelsY - offsetY) * (this.tileExtents.sizeY() / this.pixelsY);

    this.mousePixelLocation = `${this.mousePixelX}, ${this.mousePixelY}`;
    this.mouseWorldLocation = `${this.mouseWorldX.toFixed(3)}, ${this.mouseWorldY.toFixed(3)}`;

    if (this.updateFirstPointLocation) {
      this.userProfilePoint1SVG_WorldX = this.mouseWorldX;
      this.userProfilePoint1SVG_WorldY = this.mouseWorldY;

      this.userProfilePoint1SVG_CX = this.mousePixelX;
      this.userProfilePoint1SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateSecondPointLocation) {
      this.userProfilePoint2SVG_WorldX = this.mouseWorldX;
      this.userProfilePoint2SVG_WorldY = this.mouseWorldY;

      this.userProfilePoint2SVG_CX = this.mousePixelX;
      this.userProfilePoint2SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateFirstPointLocation || this.updateSecondPointLocation) {
      this.UpdateProfileLineOnMap();
    }

    // SV Profile
    if (this.updateSecondPointLocationSV) {
      this.userProfilePoint2SVG_WorldX = this.mouseWorldX;
      this.userProfilePoint2SVG_WorldY = this.mouseWorldY;

      this.userProfilePoint2SVG_CX = this.mousePixelX;
      this.userProfilePoint2SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateFirstPointLocationSV || this.updateSecondPointLocationSV) {
      this.UpdateProfileLineOnMap();
    }

    //if user pauses then get cell datum value
    if (this.showCellDatum) {
      if (this.prevMousePixelX == this.mousePixelX && this.prevMousePixelY == this.mousePixelY) {
        this.projectService.getCellDatum(this.projectUid, this.designUid, this.designOffset, this.mouseWorldX, this.mouseWorldY, this.mode)
          .subscribe(result => {
            //TODO: display nicely
            //for now just display raw value
            this.cellDatum = result.returnCode == 0 ? result.value.toFixed(1) : "";
            this.cellDatum += " (" + result.timestamp + ")";
          });
      };
      this.prevMousePixelX = this.mousePixelX;
      this.prevMousePixelY = this.mousePixelY;
    } else {
      this.cellDatum = "";
    }
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
    for (var i = 0; i < count; i++) {
      doSomething();
    }
  }

  public getTileXTimes(xTimes: number): void {
    this.timeSomething(() => this.performNTimes(() => this.getTile(), xTimes));
  }

  private updateTimerCompletionTime(): void {
    this.timerEndTime = performance.now();
    this.timerTotalTime = this.timerEndTime - this.timerStartTime;
  }

  public testJSONParameter(): void {
    var filter: CombinedFilter = new CombinedFilter();
    filter.spatialFilter = new SpatialFilter();
    filter.attributeFilter = new AttributeFilter();

    this.projectService.testJSONParameter(filter).subscribe(x => x);
  }

  public addADummySurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = "C:/temp/SomeFile.ttm";
    this.projectService.addSurveyedSurface(this.projectUid, descriptor, new Date(), this.tileExtents).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.designId;
        this.getSurveyedSurfaces();
      });
  }

  public addNewSurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.surveyedSurfaceFileName;
    this.projectService.addSurveyedSurface(this.projectUid, descriptor, this.surveyedSurfaceAsAtDate, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.designId;
        this.getSurveyedSurfaces();
      });
  }

  public getSurveyedSurfaces(): void {
    var result: SurveyedSurface[] = [];
    this.projectService.getSurveyedSurfaces(this.projectUid).subscribe(
      surveyedSurfaces => {
        surveyedSurfaces.forEach(surveyedSurface => result.push(surveyedSurface));
        this.surveyedSurfaces = result;
      });
  }

  public deleteSurveyedSurface(surveyedSurface: SurveyedSurface): void {
    this.projectService.deleteSurveyedSurface(this.projectUid, surveyedSurface.id).subscribe(
      uid => this.surveyedSurfaces.splice(this.surveyedSurfaces.indexOf(surveyedSurface), 1));
  }

  public addADummyDesignSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = "C:/temp/SomeFile.ttm";
    this.projectService.addDesignSurface(this.projectUid, descriptor).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesignSurfaces();
      });
  }

  public addNewDesignSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.designFileName;
    descriptor.designId = this.designUid;
    this.projectService.addDesignSurface(this.projectUid, descriptor).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesignSurfaces();
      });
  }

  public getDesignSurfaces(): void {
    var result: DesignSurface[] = [];
    this.projectService.getDesignSurfaces(this.projectUid).subscribe(
      designs => {
        designs.forEach(design => result.push(design));
        this.designs = result;
      });
  }

  public deleteDesignSurface(design: DesignSurface): void {
    this.projectService.deleteDesignSurface(this.projectUid, design.id).subscribe(
      uid => this.designs.splice(this.designs.indexOf(design), 1));
  }

  public selectDesign(): void {
    localStorage.setItem("designUid", this.designUid);
    localStorage.setItem("designOffset", this.designOffset.toString());
  }

  public addADummyAlignment(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = "C:/temp/SomeFile.svl";
    this.projectService.addAlignment(this.projectUid, descriptor).subscribe(
      uid => {
        this.newAlignmentGuid = uid.designId;
        this.getAlignments();
      });
  }

  public addNewAlignment(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.alignmentFileName;
    this.projectService.addAlignment(this.projectUid, descriptor).subscribe(
      uid => {
        this.newAlignmentGuid = uid.designId;
        this.getAlignments();
      });
  }

  public getAlignments(): void {
    var result: Alignment[] = [];
    this.projectService.getAlignments(this.projectUid).subscribe(
      alignments => {
        alignments.forEach(alignment => result.push(alignment));
        this.alignments = result;
      });
  }

  public deleteAlignment(alignment: Alignment): void {
    this.projectService.deleteAlignment(this.projectUid, alignment.id).subscribe(
      uid => this.alignments.splice(this.alignments.indexOf(alignment), 1));
  }

  public getMachineDesigns(): void {
    var result: MachineDesign[] = [];
    this.projectService.getMachineDesigns(this.projectUid).subscribe(
      machineDesigns => {
        machineDesigns.forEach(machineDesign => result.push(machineDesign));
        this.machineDesigns = result;
      });
  }

  public getSiteProofingRuns(): void {
    var result: SiteProofingRun[] = [];
    this.projectService.getSiteProofingRuns(this.projectUid).subscribe(siteProofingRuns => {
      siteProofingRuns.forEach(proofingRun => result.push(proofingRun));
      this.siteProofingRuns = result;
    });
  }

  public getMachines(): void {
    var result: Machine[] = [];
    this.projectService.getMachines(this.projectUid).subscribe(
      machines => {
        machines.forEach(machine => result.push(machine));
        this.machines = result;
      });
  }

  public getExistenceMapSubGridCount(): void {
    this.projectService.getExistenceMapSubGridCount(this.projectUid).subscribe(count =>
      this.existenceMapSubGridCount = count);
  }

  public getAllProjectMetadata(): void {
    var result: ISiteModelMetadata[] = [];
    this.projectService.getAllProjectMetadata().subscribe(
      metadata => {
        metadata.forEach(data => result.push(data));
        this.allProjectsMetadata = result;
        this.projectMetadata = this.allProjectsMetadata[this.getIndexOfSelectedProjectMetadata()];
        this.projectUid = this.projectMetadata.id;
      });
  }

  private getIndexOfSelectedProjectMetadata(): number {
    let projectUid = localStorage.getItem("projectUid");

    for (var i = 0; i < this.allProjectsMetadata.length; i++) {
      if (this.allProjectsMetadata[i].id === projectUid)
        return i;
    }

    localStorage.setItem("projectUid", undefined);
    localStorage.setItem("designUid", undefined);
    localStorage.setItem("designOffset", undefined);
    this.overrides = this.fetchDataService.getDefaultOverrides();
    localStorage.setItem("overrides", JSON.stringify(this.overrides));

    return -1;
  }

  public projectMetadataChanged(event: any): void {
    this.projectUid = this.projectMetadata.id;
    localStorage.setItem("projectUid", this.projectMetadata.id);
    this.selectProject();
  }

  public updateAllProjectsMetadata(): void {
    this.getAllProjectMetadata();
  }

  public updateDisplayedMachineEvents() {
    // Request the first 100 events of the selected event type from the selected site model and machine
    var result: string[] = [];

    var startDate: Date = this.machineEventsStartDate;
    if (startDate === undefined) {
      startDate = new Date(1980, 1, 1, 0, 0, 0, 0);
    }

    var endDate: Date = this.machineEventsEndDate;
    if (endDate === undefined) {
      endDate = new Date(2100, 1, 1, 0, 0, 0, 0);
    }

    this.projectService.getMachineEvents(this.projectUid, this.machine.id, this.eventType.item1,
      startDate, endDate, this.maxMachineEventsToReturn).subscribe(
        events => {
          events.forEach(event => result.push(event));
          this.machineEvents = result;
        });
  }

  public eventTypeChanged(event: any): void {
    this.updateDisplayedMachineEvents();
  }

  public machineChanged(event: any): void {
    this.updateDisplayedMachineEvents();
  }

  public updateMachineEvents(): void {
    this.updateDisplayedMachineEvents();
  }

  public switchToMutable(): void {
    this.projectService.switchToMutable().subscribe();
    this.currentGridName = "Mutable";
  }

  public switchToImmutable(): void {
    this.projectService.switchToImmutable().subscribe();
    this.currentGridName = "Immutable";
  }

 
  //Draw profile line from bottom left to top right of project 
  public drawProfileBLToTR(): void {
    this.drawProfileLineForDesign(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
  }

  //Draw profile line from top left to bottom right of project 
  public drawProfileTLToBR(): void {
    this.drawProfileLineForDesign(this.tileExtents.minX, this.tileExtents.maxY, this.tileExtents.maxX, this.tileExtents.minY);
  }

  public onMapMouseClick(): void {
    if (this.updateSecondPointLocation) {
      this.secondPointX = this.mouseWorldX;
      this.secondPointY = this.mouseWorldY;
      this.updateSecondPointLocation = false; // Uncheck the second check box

      this.userProfilePoint2SVG_CX = this.mousePixelX;
      this.userProfilePoint2SVG_CY = this.pixelsY - this.mousePixelY;

      this.drawProfileLineFromStartToEndPointsForProdData();
    }

    if (this.updateSecondPointLocationSV) {
      this.svSecondPointX = this.mouseWorldX;
      this.svSecondPointY = this.mouseWorldY;
      this.updateSecondPointLocationSV = false; // Uncheck the second check box

      this.userProfilePoint2SVG_CX = this.mousePixelX;
      this.userProfilePoint2SVG_CY = this.pixelsY - this.mousePixelY;

      this.drawProfileLineFromStartToEndPointsForSummaryVolumes(); // call SV Profile method
    }

    if (this.updateFirstPointLocation) {
      this.firstPointX = this.mouseWorldX;
      this.firstPointY = this.mouseWorldY;
      this.updateFirstPointLocation = false; // Uncheck the first check box
      this.updateSecondPointLocation = true; // Check the second check box

      this.userProfilePoint1SVG_CX = this.mousePixelX;
      this.userProfilePoint1SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateFirstPointLocationSV) { // Summary Volumes Profile
      this.svFirstPointX = this.mouseWorldX;
      this.svFirstPointY = this.mouseWorldY;
      this.updateFirstPointLocationSV = false; // Uncheck the first check box
      this.updateSecondPointLocationSV = true; // Check the second check box

      this.userProfilePoint1SVG_CX = this.mousePixelX;
      this.userProfilePoint1SVG_CY = this.pixelsY - this.mousePixelY;
    }

    this.userProfilePath = `M${this.userProfilePoint1SVG_CX},${this.userProfilePoint1SVG_CY} L${this.userProfilePoint2SVG_CX},${this.userProfilePoint2SVG_CY}`;

    if (this.showCellDatum) {
      this.projectService.getCellDatum(this.projectUid, this.designUid, this.designOffset, this.mouseWorldX, this.mouseWorldY, this.mode)
        .subscribe(result => {
          //TODO: display nicely
          //for now just display raw value
          this.cellDatum = result.returnCode == 0 ? result.value.toFixed(1) : "";
          this.cellDatum += " (" + result.timestamp + ")";
        });

      this.prevMousePixelX = this.mousePixelX;
      this.prevMousePixelY = this.mousePixelY;
    } else {
      this.cellDatum = "";
    }
  }

  public drawProfileLineFromStartToEndPointsForDesign(): void {
    this.drawProfileLineForDesign(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY);
  }


  public ProcessProfileDataVectorToSVGPolyLine(points: any[], minZ: number, maxZ: number, getValue: (point: any) => number): string {
    this.SetProfileViewExtents(points, minZ, maxZ);
    return this.drawProfileLine(points, minZ, maxZ, getValue);
  }

  private SetProfileViewExtents(points: any[], minZ: number, maxZ: number) {
    if (this.profileExtents === null)
      this.profileExtents.Set(points[0].station, minZ, points[points.length - 1].station, maxZ);
    else {
      this.profileExtents.IncludeY(minZ);
      this.profileExtents.IncludeY(maxZ);
    }
  }

  public drawProfileLineFromStartToEndPointsForProdData(): void {
      this.ClearAllSVGProfilePolylines();
      this.drawProfileLineForProdData(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY);
  }

  public drawProfileLineFromStartToEndPointsForSummaryVolumes(): void {
    this.ClearAllSVGProfilePolylines();
    this.drawProfileLineForSummaryVolumes(this.svFirstPointX, this.svFirstPointY, this.svSecondPointX, this.svSecondPointY);
  }

  // Requests a computed profile and then transforms the resulting XYZS points into a SVG Path string
  // with m move instruction at the first vertex, and at any vertex indicating a gap and line instructions
  // between all others
  public drawProfileLineForDesign(startX: number, startY: number, endX: number, endY: number) {
      return this.projectService.drawProfileLineForDesign(this.projectUid, this.designUid, this.designOffset, startX, startY, endX, endY)
          .subscribe(points => this.calculateProfileLine(points, pt => pt.z));
  }


  // Requests a computed profile and then transforms the resulting points into a SVG Path string
  // with a move instruction at the first vertex, and at any vertex indicating a gap and line instructions
  // between all others
  private drawProfileLineForProdData(startX: number, startY: number, endX: number, endY: number) {
      return this.projectService.drawProfileLineForProdData(this.projectUid, startX, startY, endX, endY, this.mode, this.designUid, this.designOffset, this.overrides)
        .subscribe(points => {
          this.calculateProfileLine(points, pt => pt.elevation);
          this.profilePoints = points;
        });
  };

 
  private drawProfileLineForSummaryVolumes(startX: number, startY: number, endX: number, endY: number) {
    return this.projectService.drawProfileLineForSummaryVolumes(this.projectUid, startX, startY, endX, endY)
      .subscribe(points => this.calculateProfileLine(points, pt => pt.z));
  }

  private calculateProfileLine(points: any[], getValue: (point: any) => number) {
      var minZ: number = this.kMaxTestElev;
      var maxZ: number = this.kMinTestElev;

      this.updateElevationRange(points, getValue, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
      this.profilePath = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, getValue);
      this.numPointInProfile = points.length;
  }

  private drawProfileLine(points: any[], minZ: number, maxZ: number, getValue: (point: any) => number): string {
      var result: string = "";
      var first: boolean = true;

      var stationRange: number = points[points.length - 1].station - points[0].station;
      var stationRatio: number = this.profileCanvasWidth / stationRange;

      var zRange = maxZ - minZ;
      var zRatio = this.profileCanvasHeight / zRange;

      points.forEach(point => {
          var value: number = getValue(point);
          if (value <= this.kMinTestElev) {
              // It's a gap...
              first = true;
          }
          else {
              result += (first ? "M" : "L") + ((point.station - points[0].station) * stationRatio).toFixed(3) + " " + ((this.profileCanvasHeight - (value - minZ) * zRatio)).toFixed(3) + " ";
              first = false;
          }
      });

      this.profileExtents.Set(points[0].station, minZ, points[points.length - 1].station, maxZ);
      return result;
  }

 
  private updateElevationRange(points: any[], getValue: (point: any) => number, minZ: number, maxZ: number, setValue: (min: number, max: number) => void) {
    points.forEach(pt => {
      var value: number = getValue(pt);
      if (value > this.kMinTestElev && value < minZ) minZ = value;
      if (value > this.kMinTestElev && value > maxZ) maxZ = value;
    });
    setValue(minZ, maxZ);
  }

  private ClearAllSVGProfilePolylines(): void {
    // Production data profile
    this.profilePath = "";
    this.numPointInProfile = 0;
    this.profilePoints = [];

    // Composite elevation profiles
    this.compositeElevationProfilePath_LastElev = "";
    this.compositeElevationProfilePath_FirstElev = "";
    this.compositeElevationProfilePath_LowestElev = "";
    this.compositeElevationProfilePath_HighestElev = "";
    this.compositeElevationProfilePath_LastCompositeElev = "";
    this.compositeElevationProfilePath_FirstCompositeElev = "";
    this.compositeElevationProfilePath_LowestCompositeElev = "";
    this.compositeElevationProfilePath_HighestCompositeElev = "";
  }

  private drawProfileLineForCompositeElevationData(startX: number, startY: number, endX: number, endY: number) {
    return this.projectService.drawProfileLineForCompositeElevations(this.projectUid, startX, startY, endX, endY)
      .subscribe(points => {
        this.ClearAllSVGProfilePolylines();

        // Compute the overall scale factor for the elevation range
        var minZ: number = this.kMaxTestElev;
        var maxZ: number = this.kMinTestElev;

        this.updateElevationRange(points, pt => pt.cellLastElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellFirstElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellLowestElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellHighestElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellLastCompositeElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellFirstCompositeElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellLowestCompositeElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });
        this.updateElevationRange(points, pt => pt.cellHighestCompositeElev, minZ, maxZ, (min, max) => { minZ = min; maxZ = max; });


        this._compositeElevationProfilePath_LastElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellLastElev);
        this._compositeElevationProfilePath_FirstElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellFirstElev);
        this._compositeElevationProfilePath_LowestElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellLowestElev);
        this._compositeElevationProfilePath_HighestElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellHighestElev);
        this._compositeElevationProfilePath_LastCompositeElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellLastCompositeElev);
        this._compositeElevationProfilePath_FirstCompositeElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellFirstCompositeElev);
        this._compositeElevationProfilePath_LowestCompositeElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellLowestCompositeElev);
        this._compositeElevationProfilePath_HighestCompositeElev = this.ProcessProfileDataVectorToSVGPolyLine(points, minZ, maxZ, pt => pt.cellHighestCompositeElev);

        this.compositeElevationProfilePath_LastElev = this._compositeElevationProfilePath_LastElev;
        this.compositeElevationProfilePath_FirstElev = this._compositeElevationProfilePath_FirstElev;
        this.compositeElevationProfilePath_LowestElev = this._compositeElevationProfilePath_LowestElev;
        this.compositeElevationProfilePath_HighestElev = this._compositeElevationProfilePath_HighestElev;
        this.compositeElevationProfilePath_LastCompositeElev = this._compositeElevationProfilePath_LastCompositeElev;
        this.compositeElevationProfilePath_FirstCompositeElev = this._compositeElevationProfilePath_FirstCompositeElev;
        this.compositeElevationProfilePath_LowestCompositeElev = this._compositeElevationProfilePath_LowestCompositeElev;
        this.compositeElevationProfilePath_HighestCompositeElev = this._compositeElevationProfilePath_HighestCompositeElev;

        this.showLastElevationProfile = true;
        this.showFirstElevationProfile = true;
        this.showLowestElevationProfile = true;
        this.showHighestElevationProfile = true;
        this.showLastCompositeElevationProfile = true;
        this.showFirstCompositeElevationProfile = true;
        this.showLowestCompositeElevationProfile = true;
        this.showHighestCompositeElevationProfile = true;
      });
  }

  public drawProfileLineFromStartToEndPointsForCompositeElevations(): void {
    this.drawProfileLineForCompositeElevationData(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY);
  }

  private updateMouseProfileLocationDetails(offsetX: number, offsetY: number): void {
    let localStation = offsetX;
    let localZ = this.pixelsY - offsetY;

    this.mouseProfileWorldStation = this.profileExtents.minX + offsetX * (this.profileExtents.sizeX() / this.profileCanvasWidth);
    this.mouseProfileWorldZ = this.profileExtents.minY + (this.pixelsY - offsetY) * (this.profileExtents.sizeY() / this.profileCanvasHeight);

    this.mouseProfilePixelLocation = `${localStation}, ${localZ}`;
    this.mouseProfileWorldLocation = `${this.mouseProfileWorldStation.toFixed(3)}, ${this.mouseProfileWorldZ.toFixed(3)}`;

    this.profileValue = "";
    this.profileValue2 = "";
    if (this.profilePoints.length > 0 && this.profilePoints[0].hasOwnProperty("value")) {
      let ratio = (this.mouseProfileWorldStation - this.profileExtents.minX) / this.profileExtents.sizeX();
      let index = Math.round(this.profilePoints.length * ratio) - 1;
      this.profileValue = this.profilePoints[index].value.toFixed(this.mode === 4 || this.mode === 14 ? 0 : 3);
      switch (this.mode) {
        case 26: 
          this.profileValue += "-" + this.profilePoints[index].value2.toFixed(3);
          //fall through to set above/below/on target
        case 13:
        case 14:
        case 20:
        case 10:
          this.profileValue2 = this.summaryTypes.find((item) => item.item1 === this.profilePoints[index].index).item2;
          break;
        default:
            break;
      }
    }
  }

  public onMouseOverProfile(event: any): void {
    this.updateMouseProfileLocationDetails(event.offsetX, event.offsetY);
  }

  public onMouseMoveProfile(event: any): void {
    this.updateMouseProfileLocationDetails(event.offsetX, event.offsetY);
  }

  public showLastElevationProfile_change() {
    this.compositeElevationProfilePath_LastElev = this.showLastElevationProfile ? "" : this._compositeElevationProfilePath_LastElev;
  }

  public showFirstElevationProfile_change() {
    this.compositeElevationProfilePath_FirstElev = this.showFirstElevationProfile ? "" : this._compositeElevationProfilePath_FirstElev;
  }

  public showLowestElevationProfile_change() {
    this.compositeElevationProfilePath_LowestElev = this.showLowestElevationProfile ? "" : this._compositeElevationProfilePath_LowestElev;
  }

  public showHighestElevationProfile_change() {
    this.compositeElevationProfilePath_HighestElev = this.showHighestElevationProfile ? "" : this._compositeElevationProfilePath_HighestElev;
  }

  public showLastCompositeElevationProfile_change() {
    this.compositeElevationProfilePath_LastCompositeElev = this.showLastCompositeElevationProfile ? "" : this._compositeElevationProfilePath_LastCompositeElev;
  }

  public showFirstCompositeElevationProfile_change() {
    this.compositeElevationProfilePath_FirstCompositeElev = this.showFirstCompositeElevationProfile ? "" : this._compositeElevationProfilePath_FirstCompositeElev;
  }

  public showLowestCompositeElevationProfile_change() {
    this.compositeElevationProfilePath_LowestCompositeElev = this.showLowestCompositeElevationProfile ? "" : this._compositeElevationProfilePath_LowestCompositeElev;
  }

  public showHighestCompositeElevationProfile_change() {
    this.compositeElevationProfilePath_HighestCompositeElev = this.showHighestCompositeElevationProfile ? "" : this._compositeElevationProfilePath_HighestCompositeElev;
  }
}


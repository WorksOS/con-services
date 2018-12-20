import { Component } from '@angular/core';
import { ProjectExtents, DesignDescriptor, SurveyedSurface, Design, Machine, ISiteModelMetadata, MachineEventType, MachineDesign, SiteProofingRun, XYZS } from './project-model';
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

  public currentGridName: string;

  public projectUid: string;
  public mode: number = 0;
  public pixelsX: number = 1000;
  public pixelsY: number = 500;

  public base64EncodedTile: string = '';

  public displayModes: DisplayMode[] = [];
  public displayMode: DisplayMode = new DisplayMode();

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

  private mouseWorldX: number = 0;
  private mouseWorldY: number = 0;

  public timerStartTime: number = performance.now();
  public timerEndTime: number = performance.now();
  public timerTotalTime: number = 0;

  public applyToViewOnly: boolean = false;

  public surveyedSurfaceFileName: string = "";
  public surveyedSurfaceAsAtDate: Date = new Date();
  public surveyedSurfaceOffset: number = 0;

  public newSurveyedSurfaceGuid: string = "";
  public surveyedSurfaces: SurveyedSurface[] = [];

  public designFileName: string = "";
  public designOffset: number = 0.0;
  public designUID: string = "";

  public newDesignGuid: string = "";
  public designs: Design[] = [];
  public machineDesigns: MachineDesign[] = [];
  public siteProofingRuns: SiteProofingRun[] = [];

  public machines: Machine[] = [];
  public machine: Machine = new Machine();

  public existenceMapSubGridCount: number = 0;

  public machineColumns: string[] = 
  ["id",
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
  "lastKnownLayerId"];

  public machineColumnNames: string[] =
   ["ID",
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
    "Last Known Layer"];

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
  
  public showLastElevationProfile              : boolean = true;
  public showFirstElevationProfile             : boolean = true;
  public showLowestElevationProfile            : boolean = true;
  public showHighestElevationProfile           : boolean = true;
  public showLastCompositeElevationProfile     : boolean = true;
  public showFirstCompositeElevationProfile    : boolean = true;
  public showLowestCompositeElevationProfile   : boolean = true;
  public showHighestCompositeElevationProfile  : boolean = true;
  
  public userProfilePath: string = "";
  public userProfilePoint1SVG_CX: Number = 0;
  public userProfilePoint1SVG_CY: Number = 0;
  public userProfilePoint2SVG_CX: Number = 0;
  public userProfilePoint2SVG_CY: Number = 0;

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
  public mouseProfileWorldStation: Number = 0.0;
  public mouseProfileWorldZ: Number = 0.0;

  public mouseProfilePixelLocation:string = '';
  public mouseProfileWorldLocation: string = '';

  public designProfileUid: string = ""

constructor(
    private projectService: ProjectService
  ) { }

  ngOnInit() { 
    this.projectService.getDisplayModes().subscribe((modes) => {
      modes.forEach(mode => this.displayModes.push(mode));
      this.displayMode = this.displayModes[0];
    });

    this.projectService.getMachineEventTypes().subscribe((types) => {
      types.forEach(type => this.eventTypes.push(type));
      this.eventType = this.eventTypes[0];
    });

    this.getAllProjectMetadata();

    this.switchToMutable();
  }

  public selectProject(): void {
    this.getProjectExtents();
    this.getExistenceMapSubGridCount();
    this.getSurveyedSurfaces();
    this.getDesigns();
    this.getMachines();
    this.getMachineDesigns();
    this.getSiteProofingRuns();

    // Sleep for half a second to allow the project extents result to come back, then zoom all
    setTimeout(() => this.zoomAll(), 250);
  }

  public setProjectToZero(): void {
    this.projectUid = "00000000-0000-0000-0000-000000000000";
  }

  public getProjectExtents(): void {
    this.projectService.getProjectExtents(this.projectUid).subscribe(extent => {
      this.projectExtents = new ProjectExtents(extent.minX, extent.minY, extent.maxX, extent.maxY);
    });

    this.projectService.getProjectDateRange(this.projectUid).subscribe(dateRange => {
      this.projectStartDate = dateRange.item1;
      this.projectEndDate = dateRange.item1;
    });
  }

  public displayModeChanged(event : any): void {
    this.mode = this.displayMode.item1;
    this.getTileXTimes(1);
  }

  public async getTileAsync(projectUid: string,
    mode: number,
    pixelsX: number,
    pixelsY: number,
    tileExtents: ProjectExtents): Promise<string> {

    var vm = this;
    return new Promise<string>((resolve) => this.projectService.getTile(projectUid, mode, pixelsX, pixelsY, tileExtents)
        .subscribe(tile => {
          vm.base64EncodedTile = 'data:image/png;base64,' + tile.tileData;
          resolve();
        }
        ));
  }

  public performNTimesSync(doSomething: (numRemaining:number) => Promise<any>, count: number): void {
      let result: Promise<any> = doSomething(count);
      result.then(() => {
      if (count > 0) {
        doSomething(count - 1);
      }
    });  
  }

  public testAsync() {
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.performNTimesSync(() => this.getTileAsync(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents), 10);
  }

  public getTile() : void {
    // If there is no project bail...
    if (this.projectUid === undefined)
      return;

    // Make sure the displayed tile extents is updated
    this.tileExtents = new ProjectExtents(this.tileExtents.minX, this.tileExtents.minY, this.tileExtents.maxX, this.tileExtents.maxY);
    this.projectService.getTile(this.projectUid, this.mode, this.pixelsX, this.pixelsY, this.tileExtents)
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
      let ratioFraction = (this.projectExtents.sizeY() / this.projectExtents.sizeX()) / (this.pixelsY / this.pixelsX) ;
      tileExtents.expand(ratioFraction - 1, 0);
    }

    // Assign modified extents into bound model 
    this.tileExtents = tileExtents;
    this.getTile();
  }

  public zoomIn(): void {
    this.tileExtents.shrink(this.zoomFactor, this.zoomFactor);
    this.getTileXTimes(1);
  }

  public zoomOut(): void {
    this.tileExtents.expand(this.zoomFactor, this.zoomFactor);
    this.getTileXTimes(1);
  }

  public panLeft(): void {
    this.tileExtents.panByFactor(-this.zoomFactor, 0.0);
    this.getTileXTimes(1);
  }

  public panRight(): void {
    this.tileExtents.panByFactor(this.zoomFactor, 0.0);
    this.getTileXTimes(1);
  }

  public panUp(): void {
    this.tileExtents.panByFactor(0, this.zoomFactor);
    this.getTileXTimes(1);
  }

  public panDown(): void {
    this.tileExtents.panByFactor(0, -this.zoomFactor);
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

  private updateMouseLocationDetails(offsetX : number, offsetY: number): void {
    this.mousePixelX = offsetX;
    this.mousePixelY = this.pixelsY - offsetY;

    this.mouseWorldX = this.tileExtents.minX + offsetX * (this.tileExtents.sizeX() / this.pixelsX);
    this.mouseWorldY = this.tileExtents.minY + (this.pixelsY - offsetY) * (this.tileExtents.sizeY() / this.pixelsY);

    this.mousePixelLocation = `${this.mousePixelX}, ${this.mousePixelY}`;
    this.mouseWorldLocation = `${this.mouseWorldX.toFixed(3)}, ${this.mouseWorldY.toFixed(3)}`;

    if (this.updateFirstPointLocation) {
      this.userProfilePoint1SVG_CX = this.mousePixelX;
      this.userProfilePoint1SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateSecondPointLocation) {
      this.userProfilePoint2SVG_CX = this.mousePixelX;
      this.userProfilePoint2SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateFirstPointLocation || this.updateSecondPointLocation) {
      this.userProfilePath = `M${this.userProfilePoint1SVG_CX},${this.userProfilePoint1SVG_CY} L${this.userProfilePoint2SVG_CX},${this.userProfilePoint2SVG_CY}`;
    }

    // SV Profile
    if (this.updateSecondPointLocationSV) {
      this.userProfilePoint2SVG_CX = this.mousePixelX;
      this.userProfilePoint2SVG_CY = this.pixelsY - this.mousePixelY;
    }

    if (this.updateFirstPointLocationSV || this.updateSecondPointLocationSV) {
      this.userProfilePath = `M${this.userProfilePoint1SVG_CX},${this.userProfilePoint1SVG_CY} L${this.userProfilePoint2SVG_CX},${this.userProfilePoint2SVG_CY}`;
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

  public addADummySurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = `C:/temp/${performance.now()}/SomeFile.ttm`;

    this.projectService.addSurveyedSurface(this.projectUid, descriptor, new Date(), this.tileExtents).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.id;
        this.getSurveyedSurfaces();
      });
  }

  public addNewSurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.surveyedSurfaceFileName;
    descriptor.offset = this.surveyedSurfaceOffset;

    this.projectService.addSurveyedSurface(this.projectUid, descriptor, this.surveyedSurfaceAsAtDate, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.id;
        this.getSurveyedSurfaces();
      });

  }

  public getSurveyedSurfaces(): void {
    var result: SurveyedSurface[] = [];
    this.projectService.getSurveyedSurfaces(this.projectUid).subscribe(
      surveyedSurfaces => {
        surveyedSurfaces.forEach(ss => result.push(ss));
        this.surveyedSurfaces = result;
      });  
  }

  public deleteSurveyedSurface(surveyedSurface : SurveyedSurface): void {
    this.projectService.deleteSurveyedSurface(this.projectUid, surveyedSurface.id).subscribe(x =>
      this.surveyedSurfaces.splice(this.surveyedSurfaces.indexOf(surveyedSurface), 1));
  }

  public addADummyDesign(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = `C:/temp/${performance.now()}/SomeFile.ttm`;

    this.projectService.addDesign(this.projectUid, descriptor, this.tileExtents).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesigns();
      });
  }

  public addNewDesign(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.designFileName;
    descriptor.offset = this.designOffset;

    this.projectService.addDesign(this.projectUid, descriptor, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesigns();
      });
  }

  public addNewDesignFromS3(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = this.designFileName;
    descriptor.designId = this.designUID;

    this.projectService.addDesignFromS3(this.projectUid, descriptor, new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newDesignGuid = uid.designId;
        this.getDesigns();
      });
  }

    public getDesigns(): void {
    var result: Design[] = [];
    this.projectService.getDesigns(this.projectUid).subscribe(
      designs => {
        designs.forEach(design => result.push(design));
        this.designs = result;
      });  
    }

  public deleteDesign(design: Design): void {
    this.projectService.deleteDesign(this.projectUid, design.id).subscribe(x =>
      this.designs.splice(this.designs.indexOf(design), 1));
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
      });
  }

  public projectMetadataChanged(event: any): void {
    this.projectUid = this.projectMetadata.id;
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
      this.machineEventsStartDate, this.machineEventsEndDate, this.maxMachineEventsToReturn).subscribe(      
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

  // Requests a computed profile and then transforms the resulting XYZS points into a SVG Path string
  // with m move instruction at the first vertex, and at any vertex indicating a gap and line instructions
  // between all others
  public drawProfileLineForDesign(startX: number, startY: number, endX: number, endY: number) {
    var profileCanvasHeight:number = 500.0;
    var profileCanvasWidth: number = 1000.0;

    var result: string = "";
    var first: boolean = true;

    return this.projectService.drawProfileLineForDesign(this.projectUid, this.designProfileUid, startX, startY, endX, endY)
      .subscribe(points =>
      {
        var stationRange:number = points[points.length - 1].station - points[0].station;
        var stationRatio:number = profileCanvasWidth / stationRange;

        var minZ:number = 100000.0;
        var maxZ:number = -100000.0;
        points.forEach(pt => { if (pt.z > -100000 && pt.z < minZ) minZ = pt.z });
        points.forEach(pt => { if (pt.z > -100000 && pt.z > maxZ) maxZ = pt.z });

        var zRange = maxZ - minZ;
        var zRatio = profileCanvasHeight / zRange;

        points.forEach(point => {
          if (point.z < -100000) {
            // It's a gap...
            first = true;
          }
          else {
            result += (first ? "M" : "L") + ((point.station - points[0].station) * stationRatio).toFixed(3) + " " + ((profileCanvasHeight - (point.z - minZ) * zRatio)).toFixed(3) + " ";
            first = false;
            }
        });

        this.profilePath = result;
        this.numPointInProfile = result.length;
        this.profileExtents.Set(points[0].station, minZ, points[points.length - 1].station, maxZ);
      });
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
  }

  public drawProfileLineFromStartToEndPointsForDesign(): void {
    this.drawProfileLineForDesign(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY);
  }

  public ComputeSVGForProfileValueVector(points: any[], getValue: (point: any) => number): string {
    var profileCanvasHeight: number = 500.0;
    var profileCanvasWidth: number = 1000.0;

    var result: string = "";
    var first: boolean = true;

    var stationRange: number = points[points.length - 1].station - points[0].station;
    var stationRatio: number = profileCanvasWidth / stationRange;

    var minZ: number = 100000.0;
    var maxZ: number = -100000.0;
    points.forEach(pt => { var value : number = getValue(pt); if (value > -100000 && value < minZ) minZ = value });
    points.forEach(pt => { var value : number = getValue(pt); if (value > -100000 && value > maxZ) maxZ = value });

    var zRange = maxZ - minZ;
    var zRatio = profileCanvasHeight / zRange;

    points.forEach(point => {
      var value: number = getValue(point);
      if (value < -100000) {
        // It's a gap...
        first = true;
      }
      else {
        result += (first ? "M" : "L") + ((point.station - points[0].station) * stationRatio).toFixed(3) + " " + ((profileCanvasHeight - (value - minZ) * zRatio)).toFixed(3) + " ";
        first = false;
      }
    });

    return result;
  }

  public ProcessProfileDataVectorToSVGPolyLine(points: any[], getValue: (point: any) => number, setResult: (theResult: string) => void) {
      var minZ: number = 100000.0;
      var maxZ: number = -100000.0;
      points.forEach(pt => { var value : number = getValue(pt); if (value > -100000 && value < minZ) minZ = value });
      points.forEach(pt => { var value : number = getValue(pt); if (value > -100000 && value > maxZ) maxZ = value });

      this.SetProfileViewExtents(points, minZ, maxZ);
      setResult(this.ComputeSVGForProfileValueVector(points, getValue));
  }

  public SetProfileViewExtents(points: any[], minZ: number, maxZ: number) {
    if (this.profileExtents === null)
      this.profileExtents.Set(points[0].station, minZ, points[points.length - 1].station, maxZ);
    else {
      this.profileExtents.IncludeY(minZ);
      this.profileExtents.IncludeY(maxZ);
    }
  }

  // Requests a computed profile and then transforms the resulting XYZS points into a SVG Path string
  // with a move instruction at the first vertex, and at any vertex indicating a gap and line instructions
  // between all others
    var profileCanvasWidth: number = 1000.0;

    var result: string = "";
    var first: boolean = true;

        var stationRange: number = points[points.length - 1].station - points[0].station;
        var stationRatio: number = profileCanvasWidth / stationRange;

        var minZ: number = 100000.0;
        var maxZ: number = -100000.0;
        points.forEach(pt => { if (pt.z > -100000 && pt.z < minZ) minZ = pt.z });
        points.forEach(pt => { if (pt.z > -100000 && pt.z > maxZ) maxZ = pt.z });

        var zRange = maxZ - minZ;
        var zRatio = profileCanvasHeight / zRange;

        points.forEach(point => {
          if (point.z < -100000) {
            // It's a gap...
            first = true;
          }
          else {
            result += (first ? "M" : "L") + ((point.station - points[0].station) * stationRatio).toFixed(3) + " " + ((profileCanvasHeight - (point.z - minZ) * zRatio)).toFixed(3) + " ";
            first = false;
          }
        });

        this.profilePath = result;
        this.numPointInProfile = result.length;
        this.profileExtents.Set(points[0].station, minZ, points[points.length - 1].station, maxZ);
      });
  }


  public drawProfileLineFromStartToEndPointsForProdData(): void {
    this.drawProfileLineForProdData(this.firstPointX, this.firstPointY, this.secondPointX, this.secondPointY,
      pt => pt.z,
      theResult => {
        this.profilePath = theResult;
        this.numPointInProfile = theResult.length;
      });
  }

        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellLastElev, theResult => this._compositeElevationProfilePath_LastElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellFirstElev, theResult => this._compositeElevationProfilePath_FirstElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellLowestElev, theResult => this._compositeElevationProfilePath_LowestElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellHighestElev, theResult => this._compositeElevationProfilePath_HighestElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellLastCompositeElev, theResult => this._compositeElevationProfilePath_LastCompositeElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellFirstCompositeElev, theResult => this._compositeElevationProfilePath_FirstCompositeElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellLowestCompositeElev, theResult => this._compositeElevationProfilePath_LowestCompositeElev = theResult);
        this.ProcessProfileDataVectorToSVGPolyLine(points, pt => pt.cellHighestCompositeElev, theResult => this._compositeElevationProfilePath_HighestCompositeElev = theResult);

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

    this.mouseProfileWorldStation = this.profileExtents.minX + offsetX * (this.profileExtents.sizeX() / 1000);
    this.mouseProfileWorldZ = this.profileExtents.minY + (this.pixelsY - offsetY) * (this.profileExtents.sizeY() / 500);

    this.mouseProfilePixelLocation = `${localStation}, ${localZ}`;
    this.mouseProfileWorldLocation = `${this.mouseProfileWorldStation.toFixed(3)}, ${this.mouseProfileWorldZ.toFixed(3)}`;
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


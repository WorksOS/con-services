export class ProjectExtents {
  minX: number = 0;
  minY: number = 0;
  maxX: number = 0;
  maxY: number = 0;

  public constructor(minX: number, minY: number, maxX: number, maxY: number) {
    this.minX = minX;
    this.minY = minY;
    this.maxX = maxX;
    this.maxY = maxY;
  }

  public toString(): string {
    return `MinX:${this.minX.toFixed(3)}, MaxX:${this.maxX.toFixed(3)}, MinY:${this.minY.toFixed(3)}, MaxY:${this.maxY.toFixed(3)}`;
  }

  public Set(minX : number, minY : number, maxX: number, maxY:number): void {
    this.minX = minX;
    this.minY = minY;
    this.maxX = maxX; 
    this.maxY = maxY;
  }

  public sizeX(): number { return this.maxX - this.minX; }

  public sizeY(): number { return this.maxY - this.minY; }

  public centerX(): number { return (this.maxX + this.minX) / 2; }

  public centerY(): number { return (this.maxY + this.minY) / 2; }

  public shrink(factorX: number, factorY: number) {
    let curSizeX = this.sizeX();
    let curSizeY = this.sizeY();

    this.minX += curSizeX * (factorX / 2);
    this.minY += curSizeY * (factorY / 2);
    this.maxX -= curSizeX * (factorX / 2);
    this.maxY -= curSizeY * (factorY / 2);
  }

  public expand(factorX: number, factorY: number) {
    let curSizeX = this.sizeX();
    let curSizeY = this.sizeY();

    this.minX -= curSizeX * (factorX / 2);
    this.minY -= curSizeY * (factorY / 2);
    this.maxX += curSizeX * (factorX / 2);
    this.maxY += curSizeY * (factorY / 2);
  }

  public panByFactor(dx_factor: number, dy_factor: number) : void {
    this.panByDelta(dx_factor * this.sizeX(), dy_factor * this.sizeY());
  }

  public panByDelta(dx: number, dy: number): void {
    this.minX += dx;
    this.minY += dy;
    this.maxX += dx;
    this.maxY += dy;
  }

  public setCenterPosition(cx: number, cy: number) {
    this.panByDelta(cx - this.centerX(), cy - this.centerY());  
  }
}

export class DesignDescriptor {
  designId : string = "";
  fileSpace: string = "";
  fileSpaceID: string = "";
  folder: string = "";
  fileName: string = "";
  offset:number = 0;
}

export class Design {
  public id: string = "";
  public designDescriptor: DesignDescriptor = new DesignDescriptor();
  public extents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
}

export class MachineDesign {
  public id: number;
  public name: string = "";
}

export class SurveyedSurface{
  id: string = "";
  designDescriptor: DesignDescriptor = new DesignDescriptor();
  asAtDate: Date = new Date();
  extents: ProjectExtents = new ProjectExtents(0, 0, 0, 0);
}

export class Machine {
  id: string = "";
  internalSiteModelMachineIndex : number;
  name : string; 
  machineType: number;
  deviceType: number;
  machineHardwareID : string;
  isJohnDoeMachine:boolean;
  lastKnownX: number;
  lastKnownY:number;
  lastKnownPositionTimeStamp: Date;
  lastKnownDesignName:string;
  lastKnownLayerId: number;
  targetValueChanges: any;
  compactionDataReported: boolean;
  compactionSensorType:number;
}

export interface ISiteModelMetadata {
  id: string;
  name: string;
  description:string;
  lastModifiedDate: Date;
  siteModelExtent: ProjectExtents;
  machineCount:number;
  designCount: number;
  surveyedSurfaceCount: number;
}

export class MachineEventType {
  item1: number;
  item2: string;
}

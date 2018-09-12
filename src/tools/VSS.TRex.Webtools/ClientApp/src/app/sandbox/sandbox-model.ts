export class ProjectExtents {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;

  public constructor(minX: number, minY: number, maxX: number, maxY: number) {
    this.minX = minX;
    this.minY = minY;
    this.maxX = maxX;
    this.maxY = maxY;
  }

  public toString(): string {
    return `MinX:${this.minX}, MaxX:${this.maxX}, MinY:${this.minY}, MaxY:${this.maxY}`;
  }

  public Set(minX : number, minY : number, maxX: number, maxY:number): void {
    this.minX = minX;
    this.minY = minY;
    this.maxX = maxX; 
    this.maxY = maxY;
  }
}


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

  public centerX(): number { return this.minX + (this.maxX + this.minX) / 2; }

  public centerY(): number { return this.minY + (this.maxY + this.minY) / 2; }

  public shrink(factor: number) {
    let curSizeX = this.sizeX();
    let curSizeY = this.sizeY();

    this.minX += curSizeX * (factor / 2);
    this.minY += curSizeY * (factor / 2);
    this.maxX -= curSizeX * (factor / 2);
    this.maxY -= curSizeY * (factor / 2);
  }

  public expand(factor: number) {
    let curSizeX = this.sizeX();
    let curSizeY = this.sizeY();

    this.minX -= curSizeX * (factor / 2);
    this.minY -= curSizeY * (factor / 2);
    this.maxX += curSizeX * (factor / 2);
    this.maxY += curSizeY * (factor / 2);
  }

  public pan(dx_factor: number, dy_factor: number) : void {
    let curSizeX = this.sizeX();
    let curSizeY = this.sizeY();

    this.minX += dx_factor * curSizeX;
    this.minY += dy_factor * curSizeY;
    this.maxX += dx_factor * curSizeX;
    this.maxY += dy_factor * curSizeY;
  }
}


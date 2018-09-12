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

  public sizeX(): number { return this.maxX - this.minX; }

  public sizeY(): number { return this.maxY - this.minY; }

  public centerX(): number { return this.minX + (this.maxX + this.minX) / 2; }

  public centerY(): number { return this.minY + (this.maxY + this.minY) / 2; }

  public shrink(factor: number) {
    this.minX += this.sizeX() * (factor / 2);
    this.minY += this.sizeY() * (factor / 2);
    this.maxX -= this.sizeX() * (factor / 2);
    this.maxY -= this.sizeY() * (factor / 2);
  }

  public expand(factor: number) {
    this.minX -= this.sizeX() * (factor / 2);
    this.minY -= this.sizeY() * (factor / 2);
    this.maxX += this.sizeX() * (factor / 2);
    this.maxY += this.sizeY() * (factor / 2);
  }

  public pan(dx_factor: number, dy_factor: number) : void {
    this.minX += dx_factor * this.sizeX();
    this.minY += dy_factor * this.sizeY();
    this.maxX += dx_factor * this.sizeX();
    this.maxY += dy_factor * this.sizeY();
  }
}


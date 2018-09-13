export class VolumeResult {
  cut: number;
  cutArea: number;
  fill: number;
  fillArea: number;
  totalCoverageArea: number;

  public toString(): string {
    return `Cut:${this.cut.toFixed(3)}, Fill:${this.fill.toFixed(3)}, Cut Area:${this.cutArea.toFixed(3)}, FillArea: ${
      this.fillArea.toFixed(3)}, Total Area:${this.totalCoverageArea.toFixed(3)}`;
    //, BoundingGrid: { BoundingExtentGrid }, BoundingLLH: { BoundingExtentLLH }`
  }

  public constructor(cut: number, cutArea: number, fill: number, fillArea: number, totalCoverageArea: number) {
    this.cut = cut;
    this.cutArea = cutArea;
    this.fill = fill;
    this.fillArea = fillArea;
    this.totalCoverageArea = totalCoverageArea;
  }
}

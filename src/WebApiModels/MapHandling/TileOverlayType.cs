namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// The types of data that can be overlayed in a map tile for reports
  /// </summary>
  public enum TileOverlayType
  {
    AllOverlays,
    BaseMap,
    ProjectBoundary,
    ProductionData,
    DxfLinework,
    Alignments,
    Geofences,
    FilterCustomBoundary,
    FilterDesignBoundary,
    FilterAlignmentBoundary,
    CutFillDesignBoundary
    //CustomBoundaries
  }
}

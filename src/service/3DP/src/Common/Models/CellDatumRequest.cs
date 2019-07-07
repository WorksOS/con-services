using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// The request to identify the cell, display information and other configuration information to determine a datum value for the cell.
  /// One of llPoint or gridPoint must be defined.
  /// </summary>
  public class CellDatumRequest : ProjectID
  {
    /// <summary>
    /// The datum type to return (eg: height, CMV, Temperature etc). 
    /// Required.
    /// </summary>
    [JsonProperty(PropertyName = "displayMode", Required = Required.Always)]
    [Required]
    public DisplayMode DisplayMode { get; private set; }

    /// <summary>
    /// If defined, the WGS84 LL position to identify the cell from. 
    /// May be null.
    /// </summary>       
    [JsonProperty(PropertyName = "llPoint", Required = Required.Default)]
    public WGSPoint LLPoint { get; private set; }

    /// <summary>
    /// If defined, the grid point in the project coordinate system to identify the cell from.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "gridPoint", Required = Required.Default)]
    public Point GridPoint { get; private set; }

    /// <summary>
    /// The filter to be used to govern selection of the cell/cell pass. 
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The ID of the filter to be used.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public long FilterId { get; private set; }

    /// <summary>
    /// The lift/layer build settings to be used.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; private set; }

    /// <summary>
    /// The descriptor identifying the surface design to be used.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "design", Required = Required.Default)]
    public DesignDescriptor Design { get; private set; }

    private CellDatumRequest()
    { }

    public CellDatumRequest(
      long projectId,
      Guid? projectUid,
      DisplayMode displayMode,
      WGSPoint llPoint,
      Point gridPoint,
      FilterResult filter,
      LiftBuildSettings liftBuildSettings,
      DesignDescriptor design)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      DisplayMode = displayMode;
      LLPoint = llPoint;
      GridPoint = gridPoint;
      Filter = filter;
      FilterId = filter?.Id ?? -1;
      LiftBuildSettings = liftBuildSettings;
      Design = design;
    }

    public override void Validate()
    {
      base.Validate();

      if (GridPoint != null)
        GridPoint.Validate();

      if (Filter != null)
        Filter.Validate();

      LiftBuildSettings?.Validate();

      if (Design != null)
        Design.Validate();
    }
  }
}

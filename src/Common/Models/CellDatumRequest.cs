using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Models
{
    /// <summary>
    /// The request to identify the cell, display information and other configuration information to determine a datum value for the cell.
    /// One of llPoint or gridPoint must be defined.
    /// </summary>
    public class CellDatumRequest : ProjectID, IValidatable
    {
      /// <summary>
      /// The datum type to return (eg: height, CMV, Temperature etc). 
      /// Required.
      /// </summary>
      [JsonProperty(PropertyName = "displayMode", Required = Required.Always)]
      [Required]
      public DisplayMode displayMode { get; private set; }

      /// <summary>
      /// If defined, the WGS84 LL position to identify the cell from. 
      /// May be null.
      /// </summary>       
      [JsonProperty(PropertyName = "llPoint", Required = Required.Default)]
      public WGSPoint llPoint { get; private set; }

      /// <summary>
      /// If defined, the grid point in the project coordinate system to identify the cell from.
      /// May be null.
      /// </summary>
      [JsonProperty(PropertyName = "gridPoint", Required = Required.Default)]
      public Point gridPoint { get; private set; }

      /// <summary>
      /// The filter to be used to govern selection of the cell/cell pass. 
      /// May be null.
      /// </summary>
      [JsonProperty(PropertyName = "filter", Required = Required.Default)]
      public Filter filter { get; private set; }

      /// <summary>
      /// The ID of the filter to be used.
      /// May be null.
      /// </summary>
      [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
      public long filterId { get; private set; }

      /// <summary>
      /// The lift/layer build settings to be used.
      /// May be null.
      /// </summary>
      [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
      public LiftBuildSettings liftBuildSettings { get; private set; }

      /// <summary>
      /// The descriptor identifying the surface design to be used.
      /// May be null.
      /// </summary>
      [JsonProperty(PropertyName = "design", Required = Required.Default)]
      public DesignDescriptor design { get; private set; }

      private CellDatumRequest ()
      { }

      public static CellDatumRequest CreateCellDatumRequest(
        long projectId, 
        DisplayMode displayMode, 
        WGSPoint llPoint, 
        Point gridPoint, 
        Filter filter, 
        long filterId, 
        LiftBuildSettings liftBuildSettings, 
        DesignDescriptor design)
      {
        return new CellDatumRequest
        {
            projectId = projectId,
            displayMode = displayMode,
            llPoint = llPoint,
            gridPoint = gridPoint,
            filter = filter,
            filterId = filterId,
            liftBuildSettings = liftBuildSettings,
            design = design
          };
      }

      public override void Validate()
      {
        base.Validate();
        if (llPoint != null)
          llPoint.Validate();

        if (gridPoint != null)
          gridPoint.Validate();

        if (filter != null)
          filter.Validate();

        if (liftBuildSettings != null)
          liftBuildSettings.Validate();

        if (design != null)
          design.Validate();
      }
    }
}
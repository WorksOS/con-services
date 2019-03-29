using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;


namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request to identify the cell, display information and other configuration information to determine a datum value for the cell.
  /// One of llPoint or gridPoint must be defined.
  /// </summary>
  public class CellDatumTRexRequest
  {
    /// <summary>
    /// The project identifier
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    public Guid ProjectUid;

    /// <summary>
    /// The datum type to return (eg: height, CMV, Temperature etc). 
    /// Required.
    /// </summary>
    [JsonProperty(PropertyName = "displayMode", Required = Required.Always)]
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
    /// The descriptor identifying the surface design to be used.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "designUid", Required = Required.Default)]
    public Guid? DesignUid { get; private set; }

    /// <summary>
    /// Flag to indicate if using grid coordinates or latitude/longitude
    /// </summary>
    [JsonIgnore]
    public bool CoordsAreGrid => GridPoint != null;

    /// <summary>
    /// Default private constructor
    /// </summary>
    private CellDatumTRexRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CellDatumTRexRequest(
      Guid projectUid,
      DisplayMode displayMode,
      WGSPoint llPoint,
      Point gridPoint,
      FilterResult filter,
      Guid? designUid)
    {
      ProjectUid = projectUid;
      DisplayMode = displayMode;
      LLPoint = llPoint;
      GridPoint = gridPoint;
      Filter = filter;
      DesignUid = designUid;
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid projectUid"));
      }

      GridPoint?.Validate();
      Filter?.Validate();

      if (GridPoint == null && LLPoint == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Either a grid or WGS84 point must be provided"));
      }
      if (GridPoint != null && LLPoint != null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Only one of grid or WGS84 point can be specified"));
      }

      if (DesignUid.HasValue && DesignUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid designUid"));
      }
    }
  }
}

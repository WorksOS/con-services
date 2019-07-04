using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  public class CellPassesTRexRequest
  {
    /// <summary>
    /// The project identifier
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    public Guid ProjectUid { get; private set; }

    /// <summary>
    /// If defined, the WGS84 LL position to identify the cell from. 
    /// May be null.
    /// </summary>       
    [JsonProperty(PropertyName = "llPoint", Required = Required.Default)]
    public WGSPoint LLPoint { get; private set; }

    /// <summary>
    /// The filter to be used to govern selection of the cell/cell pass. 
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// If defined, the grid point in the project coordinate system to identify the cell from.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "gridPoint", Required = Required.Default)]
    public Point GridPoint { get; private set; }

    /// <summary>
    /// Flag to indicate if using grid coordinates or latitude/longitude
    /// </summary>
    [JsonIgnore]
    public bool CoordsAreGrid { get; }

    public CellPassesTRexRequest()
    {
      
    }

    public CellPassesTRexRequest(
      Guid projectUid,
      Point gridPoint,
      FilterResult filter)
    {
      ProjectUid = projectUid;
      GridPoint = gridPoint;
      Filter = filter;
      CoordsAreGrid = true;
    }

    public CellPassesTRexRequest(
      Guid projectUid,
      WGSPoint llPoint,
      FilterResult filter)
    {
      ProjectUid = projectUid;
      LLPoint = llPoint;
      Filter = filter;
      CoordsAreGrid = false;
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

      Filter?.Validate();

      if (GridPoint == null && LLPoint == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Either a grid or WGS84 point must be provided"));
      }

      if (GridPoint == null && CoordsAreGrid)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Grid coordinates not correctly specified"));
      }

      if (LLPoint == null && !CoordsAreGrid)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "WGS84 point not correctly specified"));
      }
    }
  }
}

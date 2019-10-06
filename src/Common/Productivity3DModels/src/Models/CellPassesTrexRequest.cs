using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  public class CellPassesTRexRequest : TRexBaseRequest
  {
    /// <summary>
    /// If defined, the WGS84 LL position to identify the cell from. 
    /// May be null.
    /// </summary>       
    [JsonProperty(Required = Required.Default)]
    public WGSPoint LLPoint { get; private set; }

    /// <summary>
    /// If defined, the grid point in the project coordinate system to identify the cell from.
    /// May be null.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
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
      FilterResult filter,
      OverridingTargets overrides,
      LiftSettings liftSettings)
      : this(projectUid, filter, overrides, liftSettings)
    {
      GridPoint = gridPoint;
      CoordsAreGrid = true;  
    }

    public CellPassesTRexRequest(
      Guid projectUid,
      WGSPoint llPoint,
      FilterResult filter,
      OverridingTargets overrides,
      LiftSettings liftSettings) 
      : this(projectUid, filter, overrides, liftSettings)
    {
      LLPoint = llPoint;
      CoordsAreGrid = false;
    }

    private CellPassesTRexRequest(
      Guid projectUid,
      FilterResult filter,
      OverridingTargets overrides,
      LiftSettings liftSettings)
    {
      ProjectUid = projectUid;
      Filter = filter;
      CoordsAreGrid = false;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override  void Validate()
    {
      base.Validate();

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

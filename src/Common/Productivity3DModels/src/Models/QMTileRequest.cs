using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  public class QMTileRequest : RaptorHelper
  {

    /// <summary>
    /// The base filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int X { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Y { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Z { get; set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public QMTileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUId">Project ID</param>
    /// <param name="filter">Filter</param>
    /// <param name="X">tile X coordinate</param>
    /// <param name="Y">tile Y coordinate</param>
    /// <param name="Z">tile Z coordinate</param>
    public QMTileRequest(
      Guid? projectUid,
      FilterResult filter,
      int x,
      int y,
      int z)
    {
      ProjectUid = projectUid;
      Filter = filter;
      X = x;
      Y = y;
      Z = z;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      Filter?.Validate();
      if (ProjectUid == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "ProjectUid must not be null"));
      }
    }

  }
}

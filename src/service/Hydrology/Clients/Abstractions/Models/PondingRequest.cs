using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class PondingRequest
  {
    private const string DEFAULT_PONDING_FILENAME = "Ponding.png";

    /// <summary>A project unique identifier.</summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Default)]
    public Guid ProjectUid { get; set; }

    /// <summary>Filter may contain either: 1 DesignBoundary or GeofenceBoundary (else project boundary is used)</summary>
    [JsonProperty(PropertyName = "FilterUID", Required = Required.Default)]
    public Guid? FilterUid { get; set; }

    /// <summary>The resolution of resultant map i.e. cell size in real world coords meters/pixel?.</summary>
    [JsonProperty(PropertyName = "Resolution", Required = Required.Default)]
    public double Resolution { get; set; }

    /// <summary>Specifies whether the unit of length is meter or feet.</summary>
    /// todoJeannie what does this refer to (resolution?), does it matter?
    [JsonProperty(PropertyName = "IsMetric", Required = Required.Default)]
    public bool IsMetric { get; set; }

    /// <summary>Name of the resultant ponding file to be returned.</summary>
    [JsonProperty(PropertyName = "FileName", Required = Required.Default)]
    public string FileName { get; set; } = DEFAULT_PONDING_FILENAME;


    public PondingRequest()
    {
      Initialize();
    }

    private void Initialize()
    {
      ProjectUid = Guid.Empty;
      FilterUid = null;
      Resolution = double.NaN;
      IsMetric = true;
    }

    public PondingRequest(Guid projectUid, Guid? filterUid, double resolution, bool isMetric,
      string fileName = DEFAULT_PONDING_FILENAME)
    {
      Initialize();
      ProjectUid = projectUid;
      FilterUid = filterUid;
      Resolution = resolution;
      IsMetric = isMetric;
      FileName = fileName;
    }

    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2001));
      }

      if (FilterUid != null && FilterUid != Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2002));
      }

      //if (FilterUid != null && FilterUid == Guid.Empty)
      //{
      //  throw new ArgumentException($"{nameof(Validate)} Empty FilterUid.");
      //}

      if (Resolution <= 0.005 || Resolution > 1000000) // todoJeannie what should these be?
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2003));
      }

      if (string.IsNullOrEmpty(FileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2004));
      }
    }
  }
}


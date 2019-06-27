using System;
using Newtonsoft.Json;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class PondingRequest
  {
    /// <summary>A project unique identifier.</summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Default)]
    public Guid ProjectUid { get; set; }

    /// <summary>Filter may contain either: 1 DesignBoundary or GeofenceBoundary (else project boundary is used)</summary>
    [JsonProperty(PropertyName = "FilterUID", Required = Required.Default)]
    public Guid? FilterUid { get; set; }

    /// <summary>The resolution of resultant map in IsMetric i.e. 5 meters/pixel.</summary>
    [JsonProperty(PropertyName = "Resolution", Required = Required.Default)]
    public double Resolution { get; set; }

    /// <summary>Specifies whether the unit of length is meter or feet.</summary>
    /// todoJeannie what does this refer to (resolution?), does it matter?
    [JsonProperty(PropertyName = "IsMetric", Required = Required.Default)]
    public bool IsMetric { get; set; }



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

    public PondingRequest(Guid projectUid, Guid? filterUid, double resolution, bool isMetric)
    {
      Initialize();
      ProjectUid = projectUid;
      FilterUid = filterUid;
      Resolution = resolution;
      IsMetric = isMetric;
    }

    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ArgumentException($"{nameof(Validate)} Invalid ProjectUid.");
      }

      if (FilterUid != null && FilterUid == Guid.Empty)
      {
        throw new ArgumentException($"{nameof(Validate)} Empty FilterUid.");
      }

      if (Resolution <= 0 || Resolution > 1000000) // todoJeannie what should max be?
      {
        throw new ArgumentException($"{nameof(Validate)} Resolution must be > 0 and < 1,000,000.");
      }
    }
  }
}


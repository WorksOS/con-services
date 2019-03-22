using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request Elevation statistics.
  /// </summary>
  public class ElevationDataRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private ElevationDataRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public ElevationDataRequest(Guid? projectUid, FilterResult filter)
    {
      ProjectUid = projectUid;
      Filter = filter;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();
    }
  }
}

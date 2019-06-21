using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request MDP summary.
  /// </summary>
  public class TemperatureSummaryRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// Only TemperatureSettings used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private TemperatureSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TemperatureSummaryRequest(Guid? projectUid, FilterResult filter, TemperatureSettings temperatureSettings)
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(temperatureSettings: temperatureSettings);
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();
      Overrides?.Validate();
    }
  }
}

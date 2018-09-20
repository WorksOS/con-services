using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
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
    /// The various summary and target values to use in preparation of the result
    /// </summary>
    [JsonProperty(PropertyName = "temperatureSettings", Required = Required.Always)]
    [Required]
    public TemperatureSettings TemperatureSettings { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private TemperatureSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TemperatureSummaryRequest(Guid projectUid, FilterResult filter, TemperatureSettings temperatureSettings)
    {
      this.ProjectUid = projectUid;
      this.Filter = filter;
      this.TemperatureSettings = temperatureSettings;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      TemperatureSettings?.Validate();
    }
  }
}

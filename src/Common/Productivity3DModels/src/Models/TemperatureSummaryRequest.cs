using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request MDP summary.
  /// </summary>
  public class TemperatureSummaryRequest : TRexSummaryRequest
  {
    /// <summary>
    /// Default private constructor
    /// </summary>
    private TemperatureSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TemperatureSummaryRequest(Guid? projectUid, FilterResult filter, TemperatureSettings temperatureSettings, LiftSettings liftSettings)
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(temperatureSettings: temperatureSettings);
      LiftSettings = liftSettings;
    }
  }
}

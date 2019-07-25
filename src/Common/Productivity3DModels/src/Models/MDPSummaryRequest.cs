using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request MDP summary.
  /// </summary>
  public class MDPSummaryRequest : ProjectID
  {
    private const short MIN_MDP = 0;
    private const short MAX_MDP = 10000;
    private const double MIN_PERCENT_MDP = 0.0;
    private const double MAX_PERCENT_MDP = 250.0;

    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// Only MdpTarget, OverrideTargetMDP, MaxMDPPercent, MinMDPPercent used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; private set; }

    /// <summary>
    /// Settings for lift analysis
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftSettings LiftSettings { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MDPSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public MDPSummaryRequest(
      Guid? projectUid,
      FilterResult filter,
      short mdpTarget,
      bool overrideTargetMDP,
      double maxMDPPercent,
      double minMDPPercent,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(mdpTarget: mdpTarget, overrideTargetMDP: overrideTargetMDP, maxMDPPercent: maxMDPPercent, minMDPPercent: minMDPPercent);
      LiftSettings = liftSettings;
    }
  }
}

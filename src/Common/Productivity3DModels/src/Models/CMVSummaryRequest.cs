using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request CMV summary.
  /// </summary>
  public class CMVSummaryRequest : ProjectID
  {
    private const ushort MIN_CMV = 0;
    private const ushort MAX_CMV = 1500;
    private const double MIN_PERCENT_CMV = 0.0;
    private const double MAX_PERCENT_CMV = 250.0;

    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// Only CmvTarget, OverrideTargetCMV, MaxCMVPercent, MinCMVPercent used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OverridingTargets Overrides { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CMVSummaryRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CMVSummaryRequest(
      Guid? projectUid,
      FilterResult filter,
      short cmvTarget,
      bool overrideTargetCMV,
      double maxCMVPercent,
      double minCMVPercent
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Overrides = new OverridingTargets(cmvTarget: cmvTarget, overrideTargetCMV: overrideTargetCMV, maxCMVPercent: maxCMVPercent, minCMVPercent: minCMVPercent);
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

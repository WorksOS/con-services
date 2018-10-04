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
    /// The target CMV value expressed in 10ths of units.
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "cmvTarget", Required = Required.Default)]
    public short CmvTarget { get; protected set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetCMV", Required = Required.Default)]
    public bool OverrideTargetCMV { get; protected set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double MinCMVPercent { get; protected set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "maxCMVPercent", Required = Required.Default)]
    public double MaxCMVPercent { get; protected set; }

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
      CmvTarget = cmvTarget;
      OverrideTargetCMV = overrideTargetCMV;
      MaxCMVPercent = maxCMVPercent;
      MinCMVPercent = minCMVPercent;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      if (OverrideTargetCMV)
      {
        if (!(CmvTarget > 0) || !(MinCMVPercent > 0 && MaxCMVPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "CMV summary request values: if overriding Target, Target and CMV Percentage should be specified."));
        }
      }

      if (CmvTarget > 0)
      {
        if (MinCMVPercent > 0 || MaxCMVPercent > 0)
        {
          if (MinCMVPercent > MaxCMVPercent)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid CMV summary request values: must have minimum % < maximum %"));
          }
        }
      }
    }
  }
}

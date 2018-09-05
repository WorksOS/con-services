using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
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
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The target CMV value expressed in 10ths of units.
    /// </summary>
    [Range(MIN_CMV, MAX_CMV)]
    [JsonProperty(PropertyName = "cmvTarget", Required = Required.Default)]
    public short cmvTarget { get; protected set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    [JsonProperty(PropertyName = "overrideTargetCMV", Required = Required.Default)]
    public bool overrideTargetCMV { get; protected set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "minCMVPercent", Required = Required.Default)]
    public double minCMVPercent { get; protected set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    [Range(MIN_PERCENT_CMV, MAX_PERCENT_CMV)]
    [JsonProperty(PropertyName = "maxCMVPercent", Required = Required.Default)]
    public double maxCMVPercent { get; protected set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CMVSummaryRequest()
    {
    }

    /// <summary>
    /// Create an instance of the CMVSummaryRequest class.
    /// </summary>
    public static CMVSummaryRequest CreateCMVSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      short cmvTarget,
      bool overrideTargetCMV,
      double maxCMVPercent,
      double minCMVPercent
    )
    {
      return new CMVSummaryRequest
      {
        ProjectUid = projectUid,
        filter = filter,
        cmvTarget = cmvTarget,
        overrideTargetCMV = overrideTargetCMV,
        maxCMVPercent = maxCMVPercent,
        minCMVPercent = minCMVPercent
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      filter?.Validate();

      if (overrideTargetCMV)
      {
        if (!(cmvTarget > 0) || !(minCMVPercent > 0 && maxCMVPercent > 0))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "CMV summary request values: if overriding Target, Target and CMV Percentage should be specified."));
        }
      }

      if (cmvTarget > 0)
      {
        if (minCMVPercent > 0 || maxCMVPercent > 0)
        {
          if (minCMVPercent > maxCMVPercent)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid CMV summary request values: must have minimum % < maximum %"));
          }
        }
      }
    }
  }
}

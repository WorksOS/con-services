using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The request representation used to request Pass Count summary.
  /// </summary>
  public class PassCountSummaryRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    ///// <summary>
    ///// Override the Pass Count target range recorded from the machine with the value of overridingTargetPassCountRange
    ///// </summary>
    //[JsonProperty(PropertyName = "overrideTargetCMV", Required = Required.Default)]
    //public bool overrideTargetPassCountRange { get; protected set; }

    /// <summary>
    /// The global override value for target pass count range. Optional.
    /// </summary>
    [JsonProperty(PropertyName = "overridingTargetPassCountRange", Required = Required.Default)]
    public TargetPassCountRange overridingTargetPassCountRange { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    protected PassCountSummaryRequest()
    {
    }

    /// <summary>
    /// Create an instance of the PassCountSummaryRequest class.
    /// </summary>
    public static PassCountSummaryRequest CreatePassCountSummaryRequest(
      Guid projectUid,
      FilterResult filter,
      //bool overrideTargetPassCountRange,
      TargetPassCountRange overridingTargetPassCountRange
    )
    {
      return new PassCountSummaryRequest
      {
        ProjectUid = projectUid,
        filter = filter,
        //overrideTargetPassCountRange = overrideTargetPassCountRange,
        overridingTargetPassCountRange = overridingTargetPassCountRange
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      filter?.Validate();

      overridingTargetPassCountRange?.Validate();
    }
  }
}

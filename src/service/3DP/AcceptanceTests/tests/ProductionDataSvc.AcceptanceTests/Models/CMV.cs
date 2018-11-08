using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The request representation used to request both detailed and summary CMV requests.
  /// </summary>
  public class CMVRequest : RequestBase
  {
    /// <summary>
    /// The project to perform the request against
    /// </summary>
    public long projectID { get; set; }

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// The various summary and target values to use in preparation of the result
    /// </summary>
    public CMVSettings cmvSettings { get; set; }

    /// <summary>
    /// The lift build settings to use in the request.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    public long filterID { get; set; }

    /// <summary>
    /// An override start date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    public DateTime? overrideStartUTC { get; set; }

    /// <summary>
    /// An override end date that applies to the operation in conjunction with any date range specified in a filter.
    /// Value may be null
    /// </summary>
    public DateTime? overrideEndUTC { get; set; }

    /// <summary>
    /// An override set of asset IDs that applies to the operation in conjunction with any asset IDs specified in a filter.
    /// Value may be null
    /// </summary>
    public List<long> overrideAssetIds { get; set; }
  }

  /// <summary>
  /// The parameters for CMV detailed and summary computations
  /// </summary>
  public class CMVSettings
  {
    /// <summary>
    /// The target CMV value expressed in 10ths of units
    /// </summary>
    public short cmvTarget { get; set; }

    /// <summary>
    /// The maximum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    public short maxCMV { get; set; }

    /// <summary>
    /// The maximum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    public double maxCMVPercent { get; set; }

    /// <summary>
    /// The minimum CMV value to be considered 'compacted' expressed in 10ths of units
    /// </summary>
    public short minCMV { get; set; }

    /// <summary>
    /// The minimum percentage the measured CMV may be compared to the cmvTarget from the machine, or the cmvTarget override if overrideTargetCMV is true
    /// </summary>
    public double minCMVPercent { get; set; }

    /// <summary>
    /// Override the target CMV recorded from the machine with the value of cmvTarget
    /// </summary>
    public bool overrideTargetCMV { get; set; }
  }
}

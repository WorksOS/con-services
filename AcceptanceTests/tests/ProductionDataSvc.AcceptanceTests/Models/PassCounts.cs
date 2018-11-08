using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The representation of a pass counts request
  /// </summary>
  public class PassCounts : RequestBase
  {
    /// <summary>
    /// The project to perform the request against. Required
    /// </summary>
    public long? projectID { get; set; }

    /// <summary>
    /// An identifier from the caller. 
    /// </summary>
    public Guid? callId { get; set; }

    /// <summary>
    /// Setting and configuration values related to processing pass count related queries
    /// </summary>
    public PassCountSettings passCountSettings { get; set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
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
    /// </summary>
    public List<long> overrideAssetIds { get; set; }
  }

  /// <summary>
  /// Setting and configuration values related to processing pass count related queries
  /// </summary>
  public class PassCountSettings
  {
    /// <summary>
    /// Is the request for a summary or detailed analysis of passcounts
    /// </summary>
    //public bool isSummary { get; set; }

    /// <summary>
    /// The array of passcount numbers to be accounted for in the pass count analysis. 
    /// This property is not used for a summary report only for a detailed report.
    /// There must be at least one item in the array and the first item's value should be > 0. 
    /// The values do not need to be evenly spaced but must increase.
    /// </summary>
    public int[] passCounts { get; set; }
  }
}

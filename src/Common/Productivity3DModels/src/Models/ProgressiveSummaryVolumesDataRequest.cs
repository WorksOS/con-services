using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.Models.Models
{
  public class ProgressiveSummaryVolumesDataRequest : ProjectID
  {
    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public ProgressiveSummaryVolumesDataRequest(
      Guid? projectUid,
      FilterResult filter,
      Guid? baseDesignUid,
      double? baseDesignOffset,
      Guid? topDesignUid,
      double? topDesignOffset,
      VolumesType volumeCalcType,
      double? cutTolerance,
      double? fillTolerance,
      FilterResult additionalSpatialFilter,
      DateTime startDateUtc,
      DateTime endDateUtc,
      int intervalSeconds)
    {
      ProjectUid = projectUid;
      Filter = filter;
      BaseDesignUid = baseDesignUid;
      BaseDesignOffset = baseDesignOffset;
      TopDesignUid = topDesignUid;
      TopDesignOffset = topDesignOffset;
      VolumeCalcType = volumeCalcType;
      CutTolerance = cutTolerance;
      FillTolerance = fillTolerance;
      AdditionalSpatialFilter = additionalSpatialFilter;
      StartDate = startDateUtc;
      EndDate = endDateUtc;
      IntervalSeconds = intervalSeconds;
    }

    /// <summary>
    /// The prime filter to be use in progressive volume calculations
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// An additional spatial constraining filter that may be used to provide additional control over the area the summary volumes are being calculated over.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult AdditionalSpatialFilter { get; private set; }

    /// <summary>
    /// The type of volume computation to be performed as a summary volumes request
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public VolumesType VolumeCalcType { get; private set; }

    /// <summary>
    /// The unique identifier of the design surface to be used as the base or earliest surface for design-filter volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? BaseDesignUid { get; private set; }

    /// <summary>
    /// The offset for the base design if it is a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double? BaseDesignOffset { get; private set; }

    /// <summary>
    /// The unique identifier of the design surface to be used as the top or latest surface for filter-design volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? TopDesignUid { get; private set; }

    /// <summary>
    /// The offset for the top design if it is a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double? TopDesignOffset { get; private set; }

    /// <summary>
    /// Sets the cut tolerance to calculate Summary Volumes in meters
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? CutTolerance { get; private set; }

    /// <summary>
    /// Sets the fill tolerance to calculate Summary Volumes in meters
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? FillTolerance { get; private set; }

    /// <summary>
    /// The date/time at which to start calculating progressive volumes.
    /// The first progressive volume will be calculated at this date
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// The date/time at which to stop calculating progressive volumes.
    /// The last progressive volume will be calculated at or before this date according
    /// to the progressive volumes interval specified
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// The time interval between calculated progressive volumes
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public int IntervalSeconds { get; private set; }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public override void Validate()
    {
      AdditionalSpatialFilter?.Validate();
      Filter?.Validate();

      if (StartDate >= EndDate)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Start date must be earlier than end date"));
      }

      if (IntervalSeconds < 10)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Cannot query intervals less than 10 seconds"));
      }
    }
  }
}

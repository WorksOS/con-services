using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// The representation of a progressive summary volumes request
  /// </summary>
  public class ProgressiveSummaryVolumesRequest : SummaryParametersBase
  {
    /// <summary>
    /// The type of volume computation to be performed as a summary volumes request
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public VolumesType VolumeCalcType { get; private set; }

    /// <summary>
    /// The descriptor of the design surface to be used as the base or earliest surface for design-filter volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public DesignDescriptor BaseDesignDescriptor { get; private set; }

    /// <summary>
    /// The descriptor of the design surface to be used as the top or latest surface for filter-design volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public DesignDescriptor TopDesignDescriptor { get; private set; }

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
    /// The only filter used when processing a progressive volumes request for all effective volume types
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter { get; protected set; }

    /// <summary>
    /// The start date for the progressive volumes series
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// The end date for the progressive volumes series
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// The interval between successive volume requests specified in seconds
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public int IntervalSeconds { get; private set; }

    [JsonIgnore]
    public bool FiltersAreMatchingGroundToGround { get; set; }

    [JsonIgnore]
    public bool ExplicitFilters { get; set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="ProgressiveSummaryVolumesRequest"/> class from being created.
    /// </summary>
    private ProgressiveSummaryVolumesRequest()
    { }

    /// <summary>
    /// Creates a <see cref="ProgressiveSummaryVolumesRequest"/> object for use with the v2 API.
    /// </summary>
    /// <returns>New instance of <see cref="ProgressiveSummaryVolumesRequest"/>.</returns>
    public static ProgressiveSummaryVolumesRequest CreateAndValidate(long projectId, Guid? projectUid, FilterResult filter, DesignDescriptor baseDesignDescriptor, DesignDescriptor topDesignDescriptor, VolumesType volumeCalcType,
      double? cutTolerance, double? fillTolerance, FilterResult additionalSpatialFilter, DateTime startDate, DateTime endDate, int intervalSeconds)
    {
      var request = new ProgressiveSummaryVolumesRequest
      {
        ProjectId = projectId,
        ProjectUid = projectUid,
        Filter = filter,
        BaseDesignDescriptor = baseDesignDescriptor,
        TopDesignDescriptor = topDesignDescriptor,
        VolumeCalcType = volumeCalcType,
        BaseFilterId = -1,
        TopFilterId = -1,
        CutTolerance = cutTolerance,
        FillTolerance = fillTolerance,
        AdditionalSpatialFilter = additionalSpatialFilter,
        StartDate = startDate,
        EndDate = endDate,
        IntervalSeconds = intervalSeconds
      };

      request.Validate();

      return request;
    }

    public override void Validate()
    {
      LiftBuildSettings?.Validate();
      AdditionalSpatialFilter?.Validate();
      Filter?.Validate();
    }
  }
}

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// The representation of a summary volumes request
  /// </summary>
  public class SummaryVolumesRequest : SummaryParametersBase
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
    
    [JsonIgnore]
    public bool FiltersAreMatchingGroundToGround { get; set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="SummaryVolumesRequest"/> class from being created.
    /// </summary>
    private SummaryVolumesRequest()
    { }

    /// <summary>
    /// Creates a <see cref="SummaryVolumesRequest"/> object for use with the v2 API.
    /// </summary>
    /// <returns>New instance of <see cref="SummaryVolumesRequest"/>.</returns>
    public static SummaryVolumesRequest CreateAndValidate(long projectId, Guid? projectUid, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor baseDesignDescriptor, DesignDescriptor topDesignDescriptor, VolumesType volumeCalcType)
    {
      var request = new SummaryVolumesRequest
      {
        ProjectId = projectId,
        ProjectUid = projectUid,
        BaseFilter = baseFilter,
        TopFilter = topFilter,
        BaseDesignDescriptor = baseDesignDescriptor,
        TopDesignDescriptor = topDesignDescriptor,
        VolumeCalcType = volumeCalcType,
        BaseFilterId = -1,
        TopFilterId = -1
      };

      request.Validate();

      return request;
    }

    public override void Validate()
    {
      this.LiftBuildSettings?.Validate();
      this.AdditionalSpatialFilter?.Validate();
      this.TopFilter?.Validate();
      this.BaseFilter?.Validate();
    }
  }
}

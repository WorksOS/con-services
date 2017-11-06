using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Report.Models;

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
    public RaptorConverters.VolumesType VolumeCalcType { get; private set; }

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
    /// Prevents a default instance of the <see cref="SummaryVolumesRequest"/> class from being created.
    /// </summary>
    private SummaryVolumesRequest()
    { }

    /// <summary>
    /// Creates a <see cref="SummaryVolumesRequest"/> object for use with the v2 API.
    /// </summary>
    /// <returns>New instance of <see cref="SummaryVolumesRequest"/>.</returns>
    public static SummaryVolumesRequest CreateAndValidate(long projectId, Filter baseFilter, Filter topFilter, DesignDescriptor baseDesignDescriptor, DesignDescriptor topDesignDescriptor, double? cutTolerance, double? fillTolerance, RaptorConverters.VolumesType volumeCalcType)
    {
      var request = new SummaryVolumesRequest
      {
        projectId = projectId,
        baseFilter = baseFilter,
        topFilter = topFilter,
        CutTolerance = cutTolerance,
        FillTolerance = fillTolerance,
        BaseDesignDescriptor = baseDesignDescriptor,
        TopDesignDescriptor = topDesignDescriptor,
        VolumeCalcType = volumeCalcType,
        baseFilterID = -1,
        topFilterID = -1
      };

      request.Validate();

      return request;
    }

    public override void Validate()
    {
      this.liftBuildSettings?.Validate();
      this.additionalSpatialFilter?.Validate();
      this.topFilter?.Validate();
      this.baseFilter?.Validate();
    }
  }
}
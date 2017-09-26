using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{
  /// <summary>
  /// The representation of a summary volumes request
  /// </summary>
  public class SummaryVolumesRequest : SummaryParametersBase
    {
    /// <summary>
        /// The type of volume computation to be performed as a summary volumes request
        /// </summary>
        [JsonProperty(PropertyName = "volumeCalcType", Required = Required.Always)]
        [Required]
        public RaptorConverters.VolumesType volumeCalcType { get; private set; }

        /// <summary>
        /// The descriptor of the design surface to be used as the base or earliest surface for design-filter volumes
        /// </summary>
        [JsonProperty(PropertyName = "baseDesignDescriptor", Required = Required.Default)]
        public DesignDescriptor baseDesignDescriptor { get; private set; }

        /// <summary>
        /// The descriptor of the design surface to be used as the top or latest surface for filter-design volumes
        /// </summary>
        [JsonProperty(PropertyName = "topDesignDescriptor", Required = Required.Default)]
        public DesignDescriptor topDesignDescriptor { get; private set; }

        /// <summary>
        /// Sets the cut tolerance to calculate Summary Volumes in meters
        /// </summary>
        /// <value>
        /// The cut tolerance.
        /// </value>
        [JsonProperty(PropertyName = "CutTolerance", Required = Required.Default)]
        public double? CutTolerance { get; private set; }

        /// <summary>
        /// Sets the fill tolerance to calculate Summary Volumes in meters
        /// </summary>
        /// <value>
        /// The cut tolerance.
        /// </value>
        [JsonProperty(PropertyName = "FillTolerance", Required = Required.Default)]
        public double? FillTolerance { get; private set; } 


        /// <summary>
        /// Prevents a default instance of the <see cref="SummaryVolumesRequest"/> class from being created.
        /// </summary>
        private SummaryVolumesRequest()
         {
         }


        public override void Validate()
        {
          if (this.liftBuildSettings != null)
            this.liftBuildSettings.Validate();

          if (this.additionalSpatialFilter != null)
            this.additionalSpatialFilter.Validate();


          if (this.topFilter != null)
            this.topFilter.Validate();


          if (this.baseFilter != null)
            this.baseFilter.Validate();
        }
    }
}
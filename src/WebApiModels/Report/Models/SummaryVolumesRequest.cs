using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;

namespace VSS.Raptor.Service.WebApiModels.Report.Models
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

         /// <summary>
         /// Create example instance of PassCounts to display in Help documentation.
         /// </summary>
         public new static SummaryVolumesRequest HelpSample
         {
             get
             {
                 return new SummaryVolumesRequest()
                 {
                     projectId = 34,
                     additionalSpatialFilter = Filter.HelpSample,
                     additionalSpatialFilterID = 144,
                     baseDesignDescriptor = DesignDescriptor.HelpSample,
                     baseFilter = Filter.HelpSample,
                     baseFilterID = 153,
                     callId = Guid.NewGuid(),
                     liftBuildSettings = LiftBuildSettings.HelpSample,
                     topDesignDescriptor = DesignDescriptor.HelpSample,
                     topFilter = Filter.HelpSample,
                     topFilterID = 123,
                     volumeCalcType = RaptorConverters.VolumesType.Between2Filters,
                 };
             }
         }

    }



}
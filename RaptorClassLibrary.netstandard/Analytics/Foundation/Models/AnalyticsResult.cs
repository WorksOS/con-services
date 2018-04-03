using RaptorClassLibrary.netstandard.Analytics.Foundation.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Analytics.Models
{
    /// <summary>
    /// Base class for results sent to client calling contexts for analytics functions
    /// </summary>
    public class AnalyticsResult : IAnalyticsResult
    {
        public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;

        /// <summary>
        /// Populates the analytics result from the response obtained from the cluster compupe layer
        /// </summary>
        /// <param name="response"></param>
        public virtual void PopulateFromClusterComputeResponse(object response)
        {
            ResultStatus = RequestErrorStatus.OK;
        }
    }
}

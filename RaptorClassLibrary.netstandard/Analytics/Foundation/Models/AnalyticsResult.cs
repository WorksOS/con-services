using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Analytics.Models
{
    /// <summary>
    /// Base class for results sent to client calling contexts for analytics functions
    /// </summary>
    public abstract class AnalyticsResult
    {
        public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;

        /// <summary>
        /// Populates the analytics result from the response obtained from the cluster compupe layer
        /// </summary>
        /// <param name="response"></param>
        public abstract void PopulateFromClusterComputeResponse(Object response);
    }
}

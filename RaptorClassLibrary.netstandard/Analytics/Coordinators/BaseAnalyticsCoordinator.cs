using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Filters;

namespace VSS.VisionLink.Raptor.Analytics.Coordinators
{
    /// <summary>
    /// Base class used by all Analytics style operations. It defines common state and behaviour for those requests at the client context level.
    /// </summary>
    public abstract class BaseAnalyticsCoordinator<TArgument, TResponse>
    {
        /// <summary>
        /// The ID of the site model the volume is being calculated for 
        /// </summary>
        public long SiteModelID { get; set; } = -1;

        /// <summary>
        /// The filter to be used for the operation
        /// </summary>
        public CombinedFilter Filter { get; set; } = null;

        /// <summary>
        /// Execution method for the derived coordinator to override
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public abstract TResponse Execute(TArgument arg);
    }
}

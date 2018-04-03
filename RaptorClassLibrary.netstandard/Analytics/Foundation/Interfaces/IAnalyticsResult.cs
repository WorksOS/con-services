using System;
using System.Collections.Generic;
using System.Text;

namespace RaptorClassLibrary.netstandard.Analytics.Foundation.Interfaces
{
    public interface IAnalyticsResult
    {
        void PopulateFromClusterComputeResponse(object response);
    }
}

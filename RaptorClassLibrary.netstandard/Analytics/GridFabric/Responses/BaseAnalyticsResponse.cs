using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.Responses
{
    public class BaseAnalyticsResponse
    {
        public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;
    }
}

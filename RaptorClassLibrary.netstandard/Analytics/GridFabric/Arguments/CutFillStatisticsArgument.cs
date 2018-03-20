using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.GridFabric.Arguments;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments
{
    public class CutFillStatisticsArgument : BaseApplicationServiceRequestArgument
    {
        public long DataModelID;

        // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor;

        public CombinedFilter Filter;

        public Double[] Offsets;

        public long DesignID { get; set; }

        // TODO  LiftBuildSettings  :TICLiftBuildSettings;
    }
}

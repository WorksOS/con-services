using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric
{
    public class BaseMutableRaptorIgniteClass : BaseRaptorIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseMutableRaptorIgniteClass(string role) : base(RaptorGrids.RaptorMutableGridName(), role)
        {
        }
    }
}

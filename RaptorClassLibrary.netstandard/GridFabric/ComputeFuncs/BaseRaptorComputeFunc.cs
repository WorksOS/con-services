using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base class for compute funcs. This provides common aspects such as the injected Ignite instance
    /// </summary>
    public class BaseRaptorComputeFunc : BaseRaptorIgniteClass
    {
        public BaseRaptorComputeFunc()
        {
//            Debug.Assert(false, "BaseRaptorComputeFunc() may not be invoked");
        }

        /// <summary>
        /// Constructor accepting a role for the compute func that can identity a cluster group in the grid to perform the operation
        /// </summary>
        public BaseRaptorComputeFunc(string gridName, string role) : base(gridName, role)
        {
        }
    }
}

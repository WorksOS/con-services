using Apache.Ignite.Core;
using Apache.Ignite.Core.Resource;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// The base class for requests. This provides common aspects such as the injected Ignite instance
    /// </summary>
    [Serializable]
    public abstract class BaseRaptorRequest<TArgument, TResponse> : BaseRaptorIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public BaseRaptorRequest() : base()
        {
        }

        /// <summary>
        /// Constructor accepting a role for the request that may identify a cluster group of nodes in the grid
        /// </summary>
        /// <param name="role"></param>
        public BaseRaptorRequest(string gridName, string role) : base(gridName, role)
        {
        }

        public virtual TResponse Execute(TArgument arg)
        {            
            throw new NotImplementedException("BaseRaptorRequest has no implementation - don't call it!");
        }
    }
}

using System;

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
        public BaseRaptorRequest()
        {
        }

        /// <summary>
        /// Constructor accepting a role for the request that may identify a cluster group of nodes in the grid
        /// </summary>
        /// <param name="gridName"></param>
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

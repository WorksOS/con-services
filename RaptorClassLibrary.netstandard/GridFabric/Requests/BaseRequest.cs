using System;

namespace VSS.TRex.GridFabric.Requests
{
    /// <summary>
    /// The base class for requests. This provides common aspects such as the injected Ignite instance
    /// </summary>
    [Serializable]
    public abstract class BaseRequest<TArgument, TResponse> : BaseIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public BaseRequest()
        {
        }

        /// <summary>
        /// Constructor accepting a role for the request that may identify a cluster group of nodes in the grid
        /// </summary>
        /// <param name="gridName"></param>
        /// <param name="role"></param>
        public BaseRequest(string gridName, string role) : base(gridName, role)
        {
        }

        public virtual TResponse Execute(TArgument arg)
        {            
            throw new NotImplementedException("BaseRequest has no implementation - don't call it!");
        }
    }
}

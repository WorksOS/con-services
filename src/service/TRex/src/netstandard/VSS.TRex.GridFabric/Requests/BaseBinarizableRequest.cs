using System;

namespace VSS.TRex.GridFabric.Requests
{
  /// <summary>
  /// The base class for requests. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public abstract class BaseBinarizableRequest<TArgument, TResponse> : BaseBinarizableIgniteClass
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public BaseBinarizableRequest()
    {
    }

    /// <summary>
    /// Constructor accepting a role for the request that may identify a cluster group of nodes in the grid
    /// </summary>
    /// <param name="gridName"></param>
    /// <param name="role"></param>
    public BaseBinarizableRequest(string gridName, string role) : base(gridName, role)
    {
    }

    public abstract TResponse Execute(TArgument arg);
//    {
//      // No implementation in base class - complain if we are called
//      throw new NotImplementedException("BaseRequest.Execute invalid to call.");
//    }
  }
}

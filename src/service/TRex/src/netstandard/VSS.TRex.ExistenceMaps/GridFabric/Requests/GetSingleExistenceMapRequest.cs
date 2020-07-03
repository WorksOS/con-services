using System;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.GridFabric.Requests
{
  /// <summary>
  /// Represents a request that will extract and return an existence map
  /// </summary>
  public class GetSingleExistenceMapRequest : BaseExistenceMapRequest
  {
    private readonly IExistenceMapServer _server = DIContext.Obtain<IExistenceMapServer>();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public GetSingleExistenceMapRequest()
    {
    }

    /// <summary>
    /// Executes the request to retrieve an existence map given it's key and returns a deserialised bit mask subgrid tree
    /// representing the existence map
    /// </summary>
    public ISubGridTreeBitMask Execute(INonSpatialAffinityKey key) => _server.GetExistenceMap(key);

    /// <summary>
    /// Executes the request to retrieve an existence map given it's type descriptor and ID
    /// </summary>
    public ISubGridTreeBitMask Execute(Guid siteModeID, long descriptor, Guid ID) => Execute(CacheKey(siteModeID, descriptor, ID));
  }
}

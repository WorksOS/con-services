using System;
using System.Linq;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.GridFabric.Requests
{
  /// <summary>
  /// Represents a request that will extract and combine a set of existence maps into a single existence map
  /// Ideally this request is executed on the node containing the existence maps to minimise network traffic...
  /// </summary>
  public class GetCombinedExistenceMapRequest : BaseExistenceMapRequest
  {
    private readonly GetSingleExistenceMapRequest _singleRequest = new GetSingleExistenceMapRequest();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public GetCombinedExistenceMapRequest()
    {
    }

    /// <summary>
    /// Perform the request extracting all required existence maps and combine them together
    /// </summary>
    public ISubGridTreeBitMask Execute(INonSpatialAffinityKey[] keys)
    {
      ISubGridTreeBitMask combinedMask = null;

      foreach (var key in keys)
      {
        var Mask = _singleRequest.Execute(key);

        if (Mask != null)
        {
          if (combinedMask == null)
            combinedMask = Mask;
          else
            combinedMask.SetOp_OR(Mask);
        }
      }

      return combinedMask;
    }

    /// <summary>
    /// Executes the request to retrieve a combined existence map given a list of type descriptors and IDs
    /// </summary>
    public ISubGridTreeBitMask Execute(Guid siteModelID, Tuple<long, Guid>[] IDs) => Execute(IDs.Select(x => CacheKey(siteModelID, x.Item1, x.Item2)).ToArray());
  }
}

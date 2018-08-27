using System;
using System.Linq;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.GridFabric.Requests
{
  /// <summary>
  /// Represents a request that will extract and combine a set of existance maps into a single existence map
  /// Ideally this request is executed on the node containing the existance maps to minimise network traffic...
  /// </summary>
  public class GetCombinedExistenceMapRequest : BaseExistenceMapRequest
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public GetCombinedExistenceMapRequest()
    {
    }

    /// <summary>
    /// Perform the request extracting all required existence maps and combine them together
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static ISubGridTreeBitMask Execute(NonSpatialAffinityKey[] keys)
    {
      ISubGridTreeBitMask combinedMask = null;

      foreach (NonSpatialAffinityKey key in keys)
      {
        ISubGridTreeBitMask Mask = GetSingleExistenceMapRequest.Execute(key);

        if (Mask == null)
          continue;

        if (combinedMask == null)
          combinedMask = Mask;
        else
          combinedMask.SetOp_OR(Mask);
      }

      return combinedMask;
    }

    /// <summary>
    /// Executes the request to retrieve a combined existence map given a list of type descriptors and IDs
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="IDs"></param>
    /// <returns></returns>
    public static ISubGridTreeBitMask Execute(Guid siteModelID, Tuple<long, Guid>[] IDs) => Execute(IDs.Select(x => CacheKey(siteModelID, x.Item1, x.Item2)).ToArray());
  }
}

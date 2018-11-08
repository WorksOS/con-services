using System;
using VSS.TRex.ExistenceMaps.Servers;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.GridFabric.Requests
{
    /// <summary>
    /// Represents a request that will store an existence map
    /// </summary>
    public class SetExistenceMapRequest : BaseExistenceMapRequest
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SetExistenceMapRequest()
        {
        }

        /// <summary>
        /// Executes the request to store an existence map given it's key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static void Execute(INonSpatialAffinityKey key, ISubGridTreeBitMask mask) => ExistenceMapServer.Instance().SetExistenceMap(key, mask.ToBytes());

        /// <summary>
        /// Executes the request to set an existence map given it's type descriptor and ID
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="descriptor"></param>
        /// <param name="ID"></param>
        /// <param name="mask"></param>
        public static void Execute(Guid siteModelID, long descriptor, Guid ID, ISubGridTreeBitMask mask) => Execute(CacheKey(siteModelID, descriptor, ID), mask);
    }
}

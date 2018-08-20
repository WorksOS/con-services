using System;
using System.IO;
using VSS.TRex.ExistenceMaps.Servers;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Utilities;

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
        public static void Execute(NonSpatialAffinityKey key, SubGridTreeSubGridExistenceBitMask mask)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    SubGridTreePersistor.Write(mask, Consts.EXISTENCE_MAP_HEADER, Consts.EXISTENCE_MAP_VERSION, writer, null);

                    ExistenceMapServer.Instance().SetExistenceMap(key, ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Executes the request to set an existence map given it's type descriptor and ID
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="descriptor"></param>
        /// <param name="ID"></param>
        /// <param name="mask"></param>
        public static void Execute(Guid siteModelID, long descriptor, Guid ID, SubGridTreeSubGridExistenceBitMask mask) => Execute(CacheKey(siteModelID, descriptor, ID), mask);
    }
}

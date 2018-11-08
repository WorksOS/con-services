using System;
using VSS.TRex.ExistenceMaps.Servers;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps.GridFabric.Requests
{
    /// <summary>
    /// Represents a request that will extract and return an existence map
    /// </summary>
    public class GetSingleExistenceMapRequest : BaseExistenceMapRequest
    {
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
        /// <param name="key"></param>
        /// <returns></returns>
        public static ISubGridTreeBitMask Execute(INonSpatialAffinityKey key)
        {
            byte[] bytes = ExistenceMapServer.Instance().GetExistenceMap(key);

            if (bytes == null)
            {
                // There is no mask available for the key.
                return null;
            }

            ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();
            mask.FromBytes(bytes);

            return mask;
        }

        /// <summary>
        /// Executes the request to retrieve an existence map given it's type descriptor and ID
        /// </summary>
        /// <param name="siteModeID"></param>
        /// <param name="descriptor"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static ISubGridTreeBitMask Execute(Guid siteModeID, long descriptor, Guid ID) => Execute(CacheKey(siteModeID, descriptor, ID));
        
    }
}

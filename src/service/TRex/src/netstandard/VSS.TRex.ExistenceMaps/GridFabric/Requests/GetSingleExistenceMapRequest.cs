using System;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
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
        private readonly IExistenceMapServer server = DIContext.Obtain<IExistenceMapServer>();

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
        public ISubGridTreeBitMask Execute(INonSpatialAffinityKey key)
        {
            byte[] bytes = server.GetExistenceMap(key);
            ISubGridTreeBitMask mask = null;

            if (bytes != null)
            {
                mask = new SubGridTreeSubGridExistenceBitMask();
                mask.FromBytes(bytes);
            }

            return mask;
        }

        /// <summary>
        /// Executes the request to retrieve an existence map given it's type descriptor and ID
        /// </summary>
        /// <param name="siteModeID"></param>
        /// <param name="descriptor"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ISubGridTreeBitMask Execute(Guid siteModeID, long descriptor, Guid ID) => Execute(CacheKey(siteModeID, descriptor, ID));
        
    }
}

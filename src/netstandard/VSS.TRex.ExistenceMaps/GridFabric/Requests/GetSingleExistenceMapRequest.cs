﻿using System;
using System.IO;
using VSS.TRex.ExistenceMaps.Servers;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
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
        /// Executes the request to retrieve an existence map given it's key and returnsn a deserialised bit mask subgrid tree
        /// representing the existence map
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ISubGridTreeBitMask Execute(NonSpatialAffinityKey key)
        {
            byte[] bytes = ExistenceMapServer.Instance().GetExistenceMap(key);

            if (bytes == null)
            {
                // There is no mask available for the key.
                return null;
            }

            ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    SubGridTreePersistor.Read(mask, Interfaces.Consts.EXISTENCE_MAP_HEADER, Interfaces.Consts.EXISTENCE_MAP_VERSION, reader, null);
                }
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
        public static ISubGridTreeBitMask Execute(Guid siteModeID, long descriptor, Guid ID) => Execute(CacheKey(siteModeID, descriptor, ID));
        
    }
}

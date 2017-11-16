using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.ExistenceMaps.Servers;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.ExistenceMaps.GridFabric.Requests
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
        public SubGridTreeBitMask Execute(string key)
        {
            byte[] bytes = ExistenceMapServer.Instance().GetExistenceMap(key);
            SubGridTreeBitMask mask = new SubGridTreeBitMask();

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    SubGridTreePersistor.Read(mask, Consts.EXISTENCE_MAP_HEADER, Consts.EXISTENCE_MAP_VERSION, reader);
                }
            }

            return mask;
        }

        /// <summary>
        /// Executes the request to retrieve an existence map given it's type descriptor and ID
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SubGridTreeBitMask Execute(long siteModeID, long descriptor, long ID) => Execute(CacheKey(siteModeID, descriptor, ID));
        
    }
}

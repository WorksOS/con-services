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
        /// <returns></returns>
        public void Execute(string key, SubGridTreeBitMask mask)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    SubGridTreePersistor.Write(mask, EXISTENCE_MAP_HEADER, EXISTENCE_MAP_VERSION, writer);

                    ExistenceMapServer.Instance().SetExistenceMap(key, ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Executes the request to set an existence map given it's type descriptor and ID
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void Execute(long siteModelID, long descriptor, long ID, SubGridTreeBitMask mask) => Execute(CacheKey(siteModelID, descriptor, ID), mask);
    }
}

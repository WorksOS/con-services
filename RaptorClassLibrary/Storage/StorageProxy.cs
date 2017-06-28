using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Interfaces;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    ///  StorageProxy hides the implementation details of the underlying storage metaphor and provides an
    ///  IStorageProxy interface on demand.
    /// </summary>
    public static class StorageProxy
    {
        private static Object lockObj = new Object();

        private static IStorageProxy raptorInstance = null;
        private static IStorageProxy[] spatialInstance = new IStorageProxy[RaptorConfig.numSpatialProcessingDivisions];
//        private static string spatialInstanceDivision = "";

        public static IStorageProxy RaptorInstance()
        {
            if (raptorInstance == null)
            {
                raptorInstance = StorageProxyFactory.Storage(RaptorGrids.RaptorGridName());
            }

            return raptorInstance;
        }

        public static IStorageProxy SpatialInstance(uint spatialDivision)
        {
            if (spatialInstance[spatialDivision] == null)
            {
                lock (lockObj)
                {
                    if (spatialInstance[spatialDivision] == null)
                    {
                        spatialInstance[spatialDivision] = StorageProxyFactory.Storage(spatialDivision.ToString());
                    }
                }
            }

            return spatialInstance[spatialDivision];
        }
    }
}

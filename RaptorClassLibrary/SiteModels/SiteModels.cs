using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SiteModels
{
    /// <summary>
    /// SiteModels contains a map of site model/data model identifiers (long) and SiteModel instances. 
    /// It may receive messages from the Ignite layer regarding invalidation of cache items...
    /// </summary>
    public class SiteModels : Dictionary<long, SiteModel>
    {
        private static IStorageProxy StorageProxy = null;
        private static SiteModels instance = null;

        private SiteModels(IStorageProxy storageProxy)
        {
            StorageProxy = storageProxy;
        }

        public static SiteModels Instance()
        {
            if (instance == null)
            {
                instance = new SiteModels(StorageProxyFactory.Storage(RaptorGrids.RaptorGridName()));
            }

            return instance;
        }

        public SiteModel GetSiteModel(long ID) => GetSiteModel(ID, false);

        public SiteModel GetSiteModel(long ID, bool CreateIfNotExist)
        {
            SiteModel result = null;

            lock (this)
            {
                if (!TryGetValue(ID, out result))
                {
                    result = new SiteModel(ID);

                    if (result.LoadFromPersistentStore(StorageProxy) == FileSystemErrorStatus.OK)
                    {
                        Add(ID, result);
                    }
                    else
                    {
                        // The SiteModel does not exist in the store - create a new one if requested
                        if (CreateIfNotExist)
                        {
                            Add(ID, result);
                        }
                        else
                        {
                            result = null;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
        /// requiring the sitemodel to be reloaded
        /// </summary>
        /// <param name="SiteModelID"></param>
        public void SiteModelAttributesHaveChanged(long SiteModelID)
        {
            GetSiteModel(SiteModelID, false)?.LoadFromPersistentStore(StorageProxy);
        }
    }
}

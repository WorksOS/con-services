using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.ExistenceMaps.GridFabric.Requests;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.ExistenceMaps
{
    /// <summary>
    /// A static class facade around the existence map requests
    /// </summary>
    public static class ExistenceMaps
    {
        private static SetExistenceMapRequest setExistenceMapRequest = new SetExistenceMapRequest();
        private static GetSingleExistenceMapRequest getSingleExistenceMapRequest = new GetSingleExistenceMapRequest();
        private static GetCombinedExistenceMapRequest getCombinedExistenceMapRequest = new GetCombinedExistenceMapRequest();

        public static void SetExistenceMap(string key, SubGridTreeBitMask mask) => setExistenceMapRequest.Execute(key, mask);
        public static void SetExistenceMap(long siteModelID, long descriptor, long ID, SubGridTreeBitMask mask) => setExistenceMapRequest.Execute(siteModelID, descriptor, ID, mask);

        public static SubGridTreeBitMask GetSingleExistenceMap(string key) => getSingleExistenceMapRequest.Execute(key);
        public static SubGridTreeBitMask GetSingleExistenceMap(long siteModelID, long descriptor, long ID) => getSingleExistenceMapRequest.Execute(siteModelID, descriptor, ID);

        public static SubGridTreeBitMask GetCombinedExistenceMap(string[] keys) => getCombinedExistenceMapRequest.Execute(keys);
        public static SubGridTreeBitMask GetCombinedExistenceMap(long siteModelID, Tuple<long, long>[] keys) => getCombinedExistenceMapRequest.Execute(siteModelID, keys);
    }
}

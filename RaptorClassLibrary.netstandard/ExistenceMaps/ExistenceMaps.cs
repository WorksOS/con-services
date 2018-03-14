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

        public static void SetExistenceMap(string key, SubGridTreeSubGridExistenceBitMask mask) => setExistenceMapRequest.Execute(key, mask);
        public static void SetExistenceMap(long siteModelID, long descriptor, long ID, SubGridTreeSubGridExistenceBitMask mask) => setExistenceMapRequest.Execute(siteModelID, descriptor, ID, mask);

        public static SubGridTreeSubGridExistenceBitMask GetSingleExistenceMap(string key) => getSingleExistenceMapRequest.Execute(key);
        public static SubGridTreeSubGridExistenceBitMask GetSingleExistenceMap(long siteModelID, long descriptor, long ID) => getSingleExistenceMapRequest.Execute(siteModelID, descriptor, ID);

        public static SubGridTreeSubGridExistenceBitMask GetCombinedExistenceMap(string[] keys) => getCombinedExistenceMapRequest.Execute(keys);
        public static SubGridTreeSubGridExistenceBitMask GetCombinedExistenceMap(long siteModelID, Tuple<long, long>[] keys) => getCombinedExistenceMapRequest.Execute(siteModelID, keys);
    }
}

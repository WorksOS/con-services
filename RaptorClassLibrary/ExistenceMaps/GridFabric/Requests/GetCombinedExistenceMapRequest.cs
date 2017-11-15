using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor.ExistenceMaps.GridFabric.Requests
{
    /// <summary>
    /// Represents a request that will extract and combine a set of existance maps into a single existence map
    /// Ideally this request is executed on the node containing the existance maps to minimise network traffic...
    /// </summary>
    public class GetCombinedExistenceMapRequest : BaseExistenceMapRequest
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public GetCombinedExistenceMapRequest()
        {

        }

        /// <summary>
        /// Perform the request extracting all required existence maps and combine them together
        /// TODO: Potentially refactor as a ComputeFunc to place the compute with the date...
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public SubGridTreeBitMask Execute(string[] keys)
        {
            GetSingleExistenceMapRequest request = new GetSingleExistenceMapRequest();
            SubGridTreeBitMask combinedMask = new SubGridTreeBitMask(SubGridTree.SubGridTreeLevels - 1, SubGridTree.DefaultCellSize * SubGridTree.SubGridTreeDimension);

            foreach (string key in keys)
            {
                SubGridTreeBitMask Mask = request.Execute(key);

                if (Mask != null)
                {
                    combinedMask.SetOp_OR(Mask);
                }
            }

            return combinedMask;
        }

        /// <summary>
        /// Executes the request to retrieve a combined existence map given a list of type descriptors and IDs
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SubGridTreeBitMask Execute(long siteModelID, Tuple<long, long>[] IDs) => Execute(IDs.Select(x => CacheKey(siteModelID, x.Item1, x.Item2)).ToArray());
    }
}

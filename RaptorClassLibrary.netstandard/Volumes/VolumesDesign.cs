using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;

namespace VSS.VisionLink.Raptor.Volumes
{
    public class VolumesDesign
    {
        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public VolumesDesign()
        {
        }

        /// <summary>
        /// The descriptor of the design this object represents
        /// </summary>
        public DesignDescriptor DesignDescriptor = DesignDescriptor.Null();

        /// <summary>
        /// Computes the design elevations for the subgrid identitied by originCellAddress
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="originCellAddress"></param>
        /// <param name="cellSize"></param>
        /// <param name="designHeights"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public bool GetDesignHeights(long siteModelID,
                                     SubGridCellAddress originCellAddress,
                                     double cellSize,
                                     out ClientHeightLeafSubGrid designHeights,
                                     out DesignProfilerRequestResult errorCode)
        {
            return GetDesignHeights(DesignDescriptor, siteModelID, originCellAddress, cellSize, out designHeights, out errorCode);
        }

        /// <summary>
        /// Computes the design elevations for the subgrid identitied by originCellAddress
        /// </summary>
        /// <param name="DesignDescriptor"></param>
        /// <param name="siteModelID"></param>
        /// <param name="originCellAddress"></param>
        /// <param name="cellSize"></param>
        /// <param name="designHeights"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static bool GetDesignHeights(DesignDescriptor DesignDescriptor,
                                            long siteModelID,
                                            SubGridCellAddress originCellAddress,
                                            double cellSize,
                                            out ClientHeightLeafSubGrid designHeights,
                                            out DesignProfilerRequestResult errorCode)
        {
            // Query the DesignProfiler service to get the patch of elevations calculated

            errorCode = DesignProfilerRequestResult.OK;
            designHeights = null;

            /* Convert this into a request to the grid based profiling service call used in the Tiling example
               errorCode = DesignProfiler.RequestDesignElevationPatch
               (Construct_CalculateDesignElevationPatch_Args(DataModelID,
                                                             OriginCellAddress.X, OriginCellAddress.Y,
                                                             CellSize,
                                                             ADesignDescriptor,
                                                             TSubGridTreeLeafBitmapSubGridBits.FullMask),
                DesignHeights);

            */

            return errorCode == DesignProfilerRequestResult.OK;
        }
    }
}

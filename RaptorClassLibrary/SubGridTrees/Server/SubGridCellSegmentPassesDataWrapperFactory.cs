using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// Factory that creates subgrid segments that contain collections of cell passes
    /// </summary>
    public class SubGridCellSegmentPassesDataWrapperFactory : ISubGridCellSegmentPassesDataWrapperFactory
    {
        private static SubGridCellSegmentPassesDataWrapperFactory instance = null;

        /// <summary>
        /// Chooses which of the three segment cell pass wrappers should be created:
        ///  - NonStatic: Fully mutable high fidelity representation (most memory blocks allocated)
        ///  - Static: Immutable high fidelity representation (few memory blocks allocated)
        ///  - StaticCompressed: Immutable, compressed (with trivial loss level), few memory block allocated
        /// </summary>
        /// <returns></returns>
        public ISubGridCellSegmentPassesDataWrapper NewWrapper()
        {
            if (RaptorServerConfig.Instance().UseMutableCellPassSegments)
            {
                return new SubGridCellSegmentPassesDataWrapper_NonStatic();
            }

            if (RaptorServerConfig.Instance().CompressImmutableCellPassSegments)
            {
                return new SubGridCellSegmentPassesDataWrapper_StaticCompressed();
            }

            return new SubGridCellSegmentPassesDataWrapper_Static();
        }

        public static SubGridCellSegmentPassesDataWrapperFactory Instance()
        {
            if (instance == null)
            {
                instance = new SubGridCellSegmentPassesDataWrapperFactory();
            }

            return instance;
        }
    }
}

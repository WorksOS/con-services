using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    /// <summary>
    /// Factory that creates structures that contain a 'latest pass' records for cells in a subgrid or segment
    /// </summary>
    public class SubGridCellLatestPassesDataWrapperFactory
    {
        private static SubGridCellLatestPassesDataWrapperFactory instance = null;

        /// <summary>
        /// Chooses which of the three segment cell pass wrappers should be created:
        ///  - NonStatic: Fully mutable high fidelity representation (most memory blocks allocated)
        ///  - Static: Immutable high fidelity representation (few memory blocks allocated)
        ///  - StaticCompressed: Immutable, compressed (with trivial loss level), few memory block allocated
        /// </summary>
        /// <returns></returns>
        public ISubGridCellLatestPassDataWrapper NewWrapper()
        {
            if (RaptorServerConfig.Instance().UseMutableCellPassSegments)
            {
                return new SubGridCellLatestPassDataWrapper_NonStatic();
            }

            if (RaptorServerConfig.Instance().CompressImmutableCellPassSegments)
            {
                return new SubGridCellLatestPassDataWrapper_StaticCompressed();
            }

            // Note: Static and Static-Compressed  are the same for the latest pass information
            return new SubGridCellLatestPassDataWrapper_StaticCompressed();
        }

        public static SubGridCellLatestPassesDataWrapperFactory Instance()
        {
            if (instance == null)
            {
                instance = new SubGridCellLatestPassesDataWrapperFactory();
            }

            return instance;
        }
    }
}

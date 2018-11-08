using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Factory that creates structures that contain a 'latest pass' records for cells in a subgrid or segment
    /// </summary>
    public class SubGridCellLatestPassesDataWrapperFactory
    {
        private static SubGridCellLatestPassesDataWrapperFactory instance;

        /// <summary>
        /// Chooses which of the three segment cell pass wrappers should be created:
        ///  - NonStatic: Fully mutable high fidelity representation (most memory blocks allocated)
        ///  - Static: Immutable high fidelity representation (few memory blocks allocated)
        ///  - StaticCompressed: Immutable, compressed (with trivial loss level), few memory block allocated
        /// </summary>
        /// <returns></returns>
        public ISubGridCellLatestPassDataWrapper NewWrapper()
        {
            return NewWrapper(TRexServerConfig.Instance().UseMutableSpatialData,
                              TRexServerConfig.Instance().CompressImmutableSpatialData);

        }

        /// <summary>
        /// Chooses which of the three segment cell pass wrappers should be created:
        ///  - NonStatic: Fully mutable high fidelity representation (most memory blocks allocated)
        ///  - Static: Immutable high fidelity representation (few memory blocks allocated)
        ///  - StaticCompressed: Immutable, compressed (with trivial loss level), few memory block allocated
        /// </summary>
        /// <returns></returns>
        public ISubGridCellLatestPassDataWrapper NewWrapper(bool useMutableSpatialData, bool compressImmutableSpatialData)
        {
            ISubGridCellLatestPassDataWrapper result;
            if (useMutableSpatialData)
            {
                result = new SubGridCellLatestPassDataWrapper_NonStatic();
            } else if (compressImmutableSpatialData)
            {
                result = new SubGridCellLatestPassDataWrapper_StaticCompressed();
            }
            else
            {
                // Note: Static and Static-Compressed are the same for the latest pass information
                result = new SubGridCellLatestPassDataWrapper_StaticCompressed();
            }

            result.ClearPasses();
            return result;
        }

        /// <summary>
        /// Returns the singleton factory instance
        /// </summary>
        /// <returns></returns>
        public static SubGridCellLatestPassesDataWrapperFactory Instance()
        {
            return instance ?? (instance = new SubGridCellLatestPassesDataWrapperFactory());
        }
    }
}

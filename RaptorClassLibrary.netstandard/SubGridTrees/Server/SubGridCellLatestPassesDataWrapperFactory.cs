using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
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
            return NewWrapper(RaptorServerConfig.Instance().UseMutableSpatialData,
                              RaptorServerConfig.Instance().CompressImmutableSpatialData);

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
            if (useMutableSpatialData)
            {
                return new SubGridCellLatestPassDataWrapper_NonStatic();
            }

            if (compressImmutableSpatialData)
            {
                return new SubGridCellLatestPassDataWrapper_StaticCompressed();
            }

            // Note: Static and Static-Compressed are the same for the latest pass information
            return new SubGridCellLatestPassDataWrapper_StaticCompressed();
        }

        /// <summary>
        /// Returns the singleton factory instance
        /// </summary>
        /// <returns></returns>
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

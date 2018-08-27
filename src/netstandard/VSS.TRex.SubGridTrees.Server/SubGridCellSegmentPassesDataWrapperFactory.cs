using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Factory that creates subgrid segments that contain collections of cell passes
    /// </summary>
    public class SubGridCellSegmentPassesDataWrapperFactory : ISubGridCellSegmentPassesDataWrapperFactory
    {
        private static SubGridCellSegmentPassesDataWrapperFactory instance;

        /// <summary>
        /// Chooses which of the three segment cell pass wrappers should be created:
        ///  - NonStatic: Fully mutable high fidelity representation (most memory blocks allocated)
        ///  - Static: Immutable high fidelity representation (few memory blocks allocated)
        ///  - StaticCompressed: Immutable, compressed (with trivial loss level), few memory block allocated
        /// </summary>
        /// <returns></returns>
        public ISubGridCellSegmentPassesDataWrapper NewWrapper()
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
        public ISubGridCellSegmentPassesDataWrapper NewWrapper(bool useMutableSpatialData, bool compressImmutableSpatialData)
        {
            if (useMutableSpatialData)
            {
                return new SubGridCellSegmentPassesDataWrapper_NonStatic();
            }

            if (compressImmutableSpatialData)
            {
                return new SubGridCellSegmentPassesDataWrapper_StaticCompressed();
            }

            return new SubGridCellSegmentPassesDataWrapper_Static();
        }

        /// <summary>
        /// Returns the singleton factory instance
        /// </summary>
        /// <returns></returns>
        public static SubGridCellSegmentPassesDataWrapperFactory Instance() => instance ?? (instance = new SubGridCellSegmentPassesDataWrapperFactory());
    }
}

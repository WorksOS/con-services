namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// Useful constants for volumes operations
    /// </summary>
    public static class VolumesConsts
    {
        /// <summary>
        /// The default elevation delta tolerance between two surfaces for there to be a meaningful cut volume to be calculated for a cell
        /// </summary>
        public static double DEFAULT_CELL_VOLUME_CUT_TOLERANCE = 0.001;

        /// <summary>
        /// The default elevation delta tolerance between two surfaces for there to be a meaningful fill volume to be calculated for a cell
        /// </summary>
        public static double DEFAULT_CELL_VOLUME_FILL_TOLERANCE = 0.001;
    }
}

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// AreaControlSet contains a collection of tuning parameters that relate to how to relate production data
    /// cells to pixels when rendering tiles.
    /// </summary>
    public struct AreaControlSet
    {

        public bool UseIntegerAlgorithm;
        public double PixelXWorldSize;
        public double PixelYWorldSize;
        public double UserOriginX;
        public double UserOriginY;
        public double Rotation;

        public AreaControlSet(double APixelXWorldSize,
                             double APixelYWorldSize,
                             double AUserOriginX,
                             double AUserOriginY,
                             double ARotation,
            bool AUseIntegerAlgorithm)
        {
            UseIntegerAlgorithm = AUseIntegerAlgorithm;
            PixelXWorldSize = APixelXWorldSize;
            PixelYWorldSize = APixelYWorldSize;
            UserOriginX = AUserOriginX;
            UserOriginY = AUserOriginY;
            Rotation = ARotation;
        }

        public static AreaControlSet Null()
        {
            return new AreaControlSet(0, 0, 0, 0, 0, true);
        }
    }
}

namespace VSS.TRex.Types
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

        public AreaControlSet(double pixelXWorldSize,
                              double pixelYWorldSize,
                              double userOriginX,
                              double userOriginY,
                              double rotation,
                              bool useIntegerAlgorithm)
        {
            PixelXWorldSize = pixelXWorldSize;
            PixelYWorldSize = pixelYWorldSize;
            UserOriginX = userOriginX;
            UserOriginY = userOriginY;
            Rotation = rotation;
            UseIntegerAlgorithm = useIntegerAlgorithm;
    }

        public static AreaControlSet Null()
        {
            return new AreaControlSet(0, 0, 0, 0, 0, true);
        }
    }
}

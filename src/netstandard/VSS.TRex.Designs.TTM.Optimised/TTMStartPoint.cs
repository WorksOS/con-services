using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
    public struct TTMStartPoint
    {
        private double FY;
        private double FX;
        private int FTriangle;

        public double X { get { return FX; } }
        public double Y { get { return FY; } }

        public int Triangle { get { return FTriangle; } }

        public void Read(BinaryReader reader, TTMHeader header)
        {
            FY = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.NorthingOffsetValue;
            FX = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.EastingOffsetValue;

            FTriangle = Utilities.ReadInteger(reader, header.TriangleNumberSize) - 1;
        }
    }
}

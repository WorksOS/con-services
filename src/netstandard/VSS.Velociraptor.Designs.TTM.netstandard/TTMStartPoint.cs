using System.IO;

namespace VSS.TRex.Designs.TTM
{
    public class TTMStartPoint
    {
        private double FY;
        private double FX;
        private Triangle FTriangle;

        //{$ifdef LoadTTMIndices}
        //TriangleIndex: Integer;
        //{$endif}

        public TTMStartPoint(double aX, double aY, Triangle aTriangle)
        {
            FX = aX;
            FY = aY;
            FTriangle = aTriangle;
        }

        public double X { get { return FX; } }
        public double Y { get { return FY; } }

        public Triangle Triangle { get { return FTriangle; } }

        public void Write(BinaryWriter writer, TTMHeader header)
        {
            Utilities.WriteFloat(writer, Y - header.NorthingOffsetValue, header.VertexCoordinateSize);
            Utilities.WriteFloat(writer, X - header.EastingOffsetValue, header.VertexCoordinateSize);
            Utilities.WriteInteger(writer, Triangle.Tag, header.TriangleNumberSize);
        }

        public void Read(BinaryReader reader, TTMHeader header, Triangles triangles)
        {
            FY = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.NorthingOffsetValue;
            FX = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.EastingOffsetValue;

            int TriIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);

            FTriangle = (TriIndex > 0) && (TriIndex <= triangles.Count) ? triangles[TriIndex - 1] : null;

            //{$ifdef LoadTTMIndices}
            //TriangleIndex:= TriIndex;
            //{$endif}
        }
    }
}

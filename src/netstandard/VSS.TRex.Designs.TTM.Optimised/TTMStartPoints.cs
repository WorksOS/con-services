using System;
using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
    public class TTMStartPoints
    {
        public TTMStartPoint[] Items;

        public void Read(BinaryReader reader, TTMHeader header)
        {
          Items = new TTMStartPoint[header.NumberOfStartPoints];

          void ReadStartPoint(ref TTMStartPoint startPoint)
          {
            startPoint.Y = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.NorthingOffsetValue;
            startPoint.X = Utilities.ReadFloat(reader, header.VertexCoordinateSize) + header.EastingOffsetValue;

            startPoint.Triangle = Utilities.ReadInteger(reader, header.TriangleNumberSize) - 1;
          }

          try
          {
            int loopLimit = header.NumberOfStartPoints;
            for (int i = 0; i < loopLimit; i++)
            {
              long RecPos = reader.BaseStream.Position;
              ReadStartPoint(ref Items[i]);
              reader.BaseStream.Position = RecPos + header.StartPointRecordSize;
            }
          }
          catch (Exception E)
          {
            throw new Exception($"Failed to read start points\n{E}");
          }        
        }
    }
}

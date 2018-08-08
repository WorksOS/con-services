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

            for (int i = 0; i < header.NumberOfStartPoints; i++)
            {
                try
                {
                    long RecPos = reader.BaseStream.Position;
                    Items[i] = new TTMStartPoint();
                    Items[i].Read(reader, header);
                    reader.BaseStream.Position = RecPos + header.StartPointRecordSize;
                }
                catch (Exception E)
                {
                    throw new Exception($"Failed to read start point {i + 1}\n{E}");
                }
            }
        }
    }
}

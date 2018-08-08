using System;
using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
    public class TTMEdges
    {
      public int[] Items;

        public void Read(BinaryReader reader, TTMHeader header)
        {
            Items = new int[header.NumberOfEdgeRecords];

            for (int i = 0; i < header.NumberOfEdgeRecords; i++)
            {
                try
                {
                    long RecPos = reader.BaseStream.Position;
                    Items[i] = Utilities.ReadInteger(reader, header.TriangleNumberSize);

                    reader.BaseStream.Position = RecPos + header.EdgeRecordSize;
                }
                catch (Exception E)
                {
                      throw new Exception($"Failed to read edge {i + 1}\n{E}");
                }
            }       
        }
    }
}

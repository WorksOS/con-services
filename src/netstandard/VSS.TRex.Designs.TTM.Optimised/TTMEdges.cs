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

        try
        {
          int loopLimit = header.NumberOfEdgeRecords;
          for (int i = 0; i < loopLimit; i++)
          {
            long RecPos = reader.BaseStream.Position;
            Items[i] = Utilities.ReadInteger(reader, header.TriangleNumberSize) - 1;
            reader.BaseStream.Position = RecPos + header.EdgeRecordSize;
          }
        }
        catch (Exception E)
        {
          throw new Exception($"Failed to read edges\n{E}");
        }
      }
    }
}

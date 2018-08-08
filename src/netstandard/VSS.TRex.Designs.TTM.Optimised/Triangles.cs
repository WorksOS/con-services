using System;
using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
  public class Triangles
  {
    public Triangle[] Items;

    public Triangles()
    {
    }

    public void Read(BinaryReader reader, TTMHeader header)
    {
      int trinum = 0;
      Items = new Triangle[header.NumberOfTriangles];

      try
      {
        for (int i = 0; i < Items.Length; i++)
        {
          trinum = i;

          long RecPos = reader.BaseStream.Position;
          Items[i] = new Triangle
          {
            Tag = i + 1
          };

          Items[i].Read(reader, header);
          reader.BaseStream.Position = RecPos + header.TriangleRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new Exception($"Failed to read triangles at triangle {trinum + 1}\n{E}");
      }
    }
  }
}

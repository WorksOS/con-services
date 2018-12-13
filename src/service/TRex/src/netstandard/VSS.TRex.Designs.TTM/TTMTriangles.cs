using System;
using System.IO;

namespace VSS.TRex.Designs.TTM
{
  public class TTMTriangles : Triangles
  {
    public TTMTriangles() : base()
    {
      CreateTriangleFunc = (v0, v1, v2) => new TTMTriangle(v0, v1, v2);
    }

    public void Write(BinaryWriter writer, TTMHeader header)
    {
      NumberTriangles();

      foreach (TTMTriangle triangle in this)
      {
        triangle.Write(writer, header);
      }
    }

    public void Read(BinaryReader reader, TTMHeader header, TriVertices vertices)
    {
      Capacity = header.NumberOfTriangles;

      // Create objects first as we need neighbour triangles to exist
      for (int i = 0; i < header.NumberOfTriangles; i++)
      {
        Add(new TTMTriangle(null, null, null));
      }

      NumberTriangles();

      int trinum = 0;
      try
      {
        for (int i = 0; i < Count; i++)
        {
          trinum = i;
          long RecPos = reader.BaseStream.Position;
          (this[i] as TTMTriangle).Read(reader, header, vertices, this, i + 1);
          reader.BaseStream.Position = RecPos + header.TriangleRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new Exception($"Failed to read triangle {trinum + 1}", E);
      }
    }
  }
}

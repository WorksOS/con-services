using System;
using System.IO;

namespace VSS.TRex.Designs.TTM.Optimised
{
  public class TriVertices
  {
    public TriVertex[] Items;

    /// <summary>
    /// Base no-arg constructor
    /// </summary>
    public TriVertices()
    {
    }

    public void Read(BinaryReader reader, TTMHeader header)
    {
      int vertnum = 0;
      Items = new TriVertex[header.NumberOfVertices];

      try
      {
        for (int i = 0; i < header.NumberOfVertices; i++)
        {
          vertnum = i;
          long RecPos = reader.BaseStream.Position;
          Items[i] = new TriVertex
          {
            Tag = i + 1
          };
          Items[i].Read(reader, header);
          reader.BaseStream.Position = RecPos + header.VertexRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new Exception($"Failed to read vertex {vertnum + 1}\n{E}");
      }
    }
  }
}

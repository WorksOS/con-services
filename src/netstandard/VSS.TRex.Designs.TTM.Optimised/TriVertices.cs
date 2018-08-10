using System;
using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised
{
  public class TriVertices
  {
    public XYZ[] Items;

    /// <summary>
    /// Base no-arg constructor
    /// </summary>
    public TriVertices()
    {
    }

    public void Read(BinaryReader reader, TTMHeader header)
    {
      bool readCoordinateFloats = header.VertexCoordinateSize == sizeof(float);
      bool readVertexValueFloats = header.VertexValueSize == sizeof(float);

      Items = new XYZ[header.NumberOfVertices];

      void ReadVertex(ref XYZ vertex)
      {
          if (readCoordinateFloats)
          {
            vertex.Y = reader.ReadSingle() + header.NorthingOffsetValue;
            vertex.X = reader.ReadSingle() + header.EastingOffsetValue;
          }
          else
          {
            vertex.Y = reader.ReadDouble() + header.NorthingOffsetValue;
            vertex.X = reader.ReadDouble() + header.EastingOffsetValue;
          }
         
          vertex.Z = readVertexValueFloats ? reader.ReadSingle() : (float)reader.ReadDouble();
      }

      try
      {
        int loopLimit = header.NumberOfVertices;
        for (int i = 0; i < loopLimit; i++)
        {
          long RecPos = reader.BaseStream.Position;
          ReadVertex(ref Items[i]);
          reader.BaseStream.Position = RecPos + header.VertexRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new Exception($"Failed to read vertices\n{E}");
      }
    }
  }
}

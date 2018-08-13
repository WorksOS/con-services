using System;
using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Contains the collectio of vertices used to form triangles in the TIN mesh
  /// </summary>
  public class TriVertices
  {
    /// <summary>
    /// The collection of 3D points comprising the vertices.
    /// </summary>
    public XYZ[] Items;

    /// <summary>
    /// Base no-arg constructor
    /// </summary>
    public TriVertices()
    {
    }

    /// <summary>
    /// Reads the collection of vertices usign the provided reader
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
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

        vertex.Z = readVertexValueFloats ? reader.ReadSingle() : (float) reader.ReadDouble();
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

using System;
using System.IO;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Contains the collection of vertices used to form triangles in the TIN mesh
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

      try
      {
        int loopLimit = header.NumberOfVertices;
        for (int i = 0; i < loopLimit; i++)
        {
          long RecPos = reader.BaseStream.Position;

          if (readCoordinateFloats)
          {
            Items[i].Y = reader.ReadSingle() + header.NorthingOffsetValue;
            Items[i].X = reader.ReadSingle() + header.EastingOffsetValue;
          }
          else
          {
            Items[i].Y = reader.ReadDouble() + header.NorthingOffsetValue;
            Items[i].X = reader.ReadDouble() + header.EastingOffsetValue;
          }

          Items[i].Z = readVertexValueFloats ? reader.ReadSingle() : reader.ReadDouble();

          //ReadVertex(ref Items[i]);
          reader.BaseStream.Position = RecPos + header.VertexRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new TTMFileReadException($"Failed to read vertices", E);
      }
    }

    /**** Experimental code requiring unsafe code. Removed for now...
    /// <summary>
    /// Reads the collection of vertices usign the provided reader
    /// </summary>
    /// <param name="header"></param>
    public unsafe void Read(byte[] bytes, int bufPos, TTMHeader header)
    {
      bool readCoordinateFloats = header.VertexCoordinateSize == sizeof(float);
      bool readVertexValueFloats = header.VertexValueSize == sizeof(float);

      Items = new XYZ[header.NumberOfVertices];

      try
      {
        ulong tmpBufferLong;
        uint tmpBufferInt;
        uint lo;
        uint hi;
        // float value; Used to test reading as a float without the double to float conversion [this takes most of the runtime here!!!]

        int loopLimit = header.NumberOfVertices;
        for (int i = 0; i < loopLimit; i++)
        {
          int RecPos = bufPos;

          if (readCoordinateFloats)
          {
            tmpBufferInt = (uint) (bytes[RecPos] | bytes[RecPos + 1] << 8 | bytes[RecPos + 2] << 16 | bytes[RecPos + 3] << 24);
            Items[i].Y = *((float*) &tmpBufferInt) + header.NorthingOffsetValue;
//            Items[i].extra = *((float*)&tmpBufferInt);

            tmpBufferInt = (uint) (bytes[RecPos + 4] | bytes[RecPos + 5] << 8 | bytes[RecPos + 6] << 16 | bytes[RecPos + 7] << 24);
            Items[i].X = *((float*) &tmpBufferInt) + header.EastingOffsetValue;
//            Items[i].extra = *((float*) &tmpBufferInt);

            RecPos += 8;
            // Items[i].Y = reader.ReadSingle() + header.NorthingOffsetValue;
            // Items[i].X = reader.ReadSingle() + header.EastingOffsetValue;
          }
          else
          {
            lo = (uint) (bytes[RecPos] | bytes[RecPos + 1] << 8 | bytes[RecPos + 2] << 16 | bytes[RecPos + 3] << 24);
            hi = (uint) (bytes[RecPos + 4] | bytes[RecPos + 5] << 8 | bytes[RecPos + 6] << 16 | bytes[RecPos + 7] << 24);

            tmpBufferLong = (ulong) hi << 32 | lo;
            Items[i].Y = *((double*) &tmpBufferLong) + header.NorthingOffsetValue;

            RecPos += 8;

            lo = (uint) (bytes[RecPos] | bytes[RecPos + 1] << 8 | bytes[RecPos + 2] << 16 | bytes[RecPos + 3] << 24);
            hi = (uint) (bytes[RecPos + 4] | bytes[RecPos + 5] << 8 | bytes[RecPos + 6] << 16 | bytes[RecPos + 7] << 24);

            tmpBufferLong = (ulong) hi << 32 | lo;
            Items[i].X = *((double*) &tmpBufferLong) + header.EastingOffsetValue;

            RecPos += 8;

            // Items[i].Y = reader.ReadDouble() + header.NorthingOffsetValue;
            // Items[i].X = reader.ReadDouble() + header.EastingOffsetValue;
          }

          if (readVertexValueFloats)
          {
            tmpBufferInt = (uint) (bytes[RecPos] | bytes[RecPos + 1] << 8 | bytes[RecPos + 2] << 16 | bytes[RecPos + 3] << 24);
            Items[i].Z = *((float*) &tmpBufferInt);
          }
          else
          {
            lo = (uint) (bytes[RecPos] | bytes[RecPos + 1] << 8 | bytes[RecPos + 2] << 16 | bytes[RecPos + 3] << 24);
            hi = (uint) (bytes[RecPos + 4] | bytes[RecPos + 5] << 8 | bytes[RecPos + 6] << 16 | bytes[RecPos + 7] << 24);

            tmpBufferLong = (ulong) hi << 32 | lo;
            Items[i].Z = *((double*) &tmpBufferLong);
          }

          // Items[i].Z = readVertexValueFloats ? reader.ReadSingle() : (float)reader.ReadDouble();

          bufPos = RecPos + header.VertexRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new TTMFileReadException($"Failed to read vertices", E);
      }
    }
    */
  }
}

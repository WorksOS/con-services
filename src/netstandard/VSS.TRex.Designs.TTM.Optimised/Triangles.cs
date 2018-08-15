using System;
using System.IO;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Defines the collection of triangles that make up this surface
  /// </summary>
  public class Triangles
  {
    /// <summary>
    /// The actual array of triangles
    /// </summary>
    public Triangle[] Items;

    public Triangles()
    {
    }

    /* From https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/IO/BinaryReader.cs
  
        public virtual unsafe float ReadSingle()
        {
            FillBuffer(4);
            uint tmpBuffer = (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
            return *((float*)&tmpBuffer);
        }

      public virtual unsafe double ReadDouble()
    {
      FillBuffer(8);
      uint lo = (uint)(_buffer[0] | _buffer[1] << 8 |
                       _buffer[2] << 16 | _buffer[3] << 24);
      uint hi = (uint)(_buffer[4] | _buffer[5] << 8 |
                       _buffer[6] << 16 | _buffer[7] << 24);

      ulong tmpBuffer = ((ulong)hi) << 32 | lo;
      return *((double*)&tmpBuffer);
    }

     public virtual short ReadInt16()
        {
            FillBuffer(2);
            return (short)(_buffer[0] | _buffer[1] << 8);
        }

        [CLSCompliant(false)]
        public virtual ushort ReadUInt16()
        {
            FillBuffer(2);
            return (ushort)(_buffer[0] | _buffer[1] << 8);
        }

        public virtual int ReadInt32()
        {
            if (_isMemoryStream)
            {
                if (_stream == null)
                {
                    throw Error.GetFileNotOpen();
                }

                // read directly from MemoryStream buffer
                MemoryStream mStream = _stream as MemoryStream;
                Debug.Assert(mStream != null, "_stream as MemoryStream != null");

                return mStream.InternalReadInt32();
            }
            else
            {
                FillBuffer(4);
                return (int)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
            }
        }
    */

    /// <summary>
    /// Reads the set of triangles in the model utilising the given reader
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
    public void Read(BinaryReader reader, TTMHeader header)
    {
      Items = new Triangle[header.NumberOfTriangles];
      bool readInt16s = header.VertexNumberSize == sizeof(short);

      void Read(ref Triangle tri)
      {
        if (readInt16s)
        {
          tri.Vertex0 = reader.ReadInt16() - 1;
          tri.Vertex1 = reader.ReadInt16() - 1;
          tri.Vertex2 = reader.ReadInt16() - 1;
        }
        else
        {
          tri.Vertex0 = reader.ReadInt32() - 1;
          tri.Vertex1 = reader.ReadInt32() - 1;
          tri.Vertex2 = reader.ReadInt32() - 1;
        }

        // This loop does not need to be executed since the reader repositions the reading location after each serialise in
        //for (int i = 0; i < 3; i++)
        //{
        //  int NeighbourIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);
        // SetNeighbour(i, (NeighbourIndex < 1 || NeighbourIndex > triangles.Items.Length) ? null : triangles.Items[NeighbourIndex - 1]);
        //}
      }

      try
      {
        int loopLimit = header.NumberOfTriangles;
        for (int i = 0; i < loopLimit; i++)
        {
          long RecPos = reader.BaseStream.Position;
          Read(ref Items[i]);
          reader.BaseStream.Position = RecPos + header.TriangleRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new TTMFileReadException($"Failed to read triangles", E);
      }
    }

    /// <summary>
    /// Reads the set of triangles in the model utilising the given reader
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="bufPos"></param>
    /// <param name="header"></param>
    public void Read(byte[] bytes, int bufPos, TTMHeader header)
    {
      Items = new Triangle[header.NumberOfTriangles];
      bool readInt16s = header.VertexNumberSize == sizeof(short);

      try
      {
        int loopLimit = header.NumberOfTriangles;
        for (int i = 0; i < loopLimit; i++)
        {
          if (readInt16s)
          {
            Items[i].Vertex0 = (bytes[bufPos] | bytes[bufPos + 1] << 8) - 1;
            Items[i].Vertex1 = (bytes[bufPos + 2] | bytes[bufPos + 3] << 8) - 1;
            Items[i].Vertex2 = (bytes[bufPos + 4] | bytes[bufPos + 5] << 8) - 1;
          }
          else
          {
            Items[i].Vertex0 = (bytes[bufPos] | bytes[bufPos + 1] << 8 | bytes[bufPos + 2] << 16 | bytes[bufPos + 3] << 24) - 1;
            Items[i].Vertex1 = (bytes[bufPos + 4] | bytes[bufPos + 5] << 8 | bytes[bufPos + 6] << 16 | bytes[bufPos + 7] << 24) - 1;
            Items[i].Vertex2 = (bytes[bufPos + 8] | bytes[bufPos + 9] << 8 | bytes[bufPos + 10] << 16 | bytes[bufPos + 11] << 24) - 1;
          }

          // This loop does not need to be executed since the reader repositions the reading location after each serialise in
          //for (int i = 0; i < 3; i++)
          //{
          //  int NeighbourIndex = Utilities.ReadInteger(reader, header.TriangleNumberSize);
          // SetNeighbour(i, (NeighbourIndex < 1 || NeighbourIndex > triangles.Items.Length) ? null : triangles.Items[NeighbourIndex - 1]);
          //}

          bufPos += header.TriangleRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new Exception($"Failed to read triangles\n{E}");
      }
    }

  }
}

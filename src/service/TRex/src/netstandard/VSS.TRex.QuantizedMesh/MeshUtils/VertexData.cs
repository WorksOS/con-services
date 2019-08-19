using System;
using System.IO;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public class VertexData
  {

    // Immediately following the header is the vertex data.An unsigned int is a 32-bit unsigned integer and an unsigned short is a 16-bit unsigned integer.
    // The vertexCount field indicates the size of the three arrays that follow.The three arrays contain the delta from the previous value that is then zig-zag encoded in order
    // to make small integers, regardless of their sign, use a small number of bits. Decoding a value is straightforward:
    /*
     *
     u = The horizontal coordinate of the vertex in the tile. When the u value is 0, the vertex is on the Western edge of the tile. When the value is 32767, the vertex is on the Eastern edge of the tile.
         For other values, the vertex's longitude is a linear interpolation between the longitudes of the Western and Eastern edges of the tile.       
         
     v = The vertical coordinate of the vertex in the tile. When the v value is 0, the vertex is on the Southern edge of the tile. When the value is 32767, the vertex is on the Northern edge of the tile.
         For other values, the vertex's latitude is a linear interpolation between the latitudes of the Southern and Nothern edges of the tile.

     height = The height of the vertex in the tile. When the height value is 0, the vertex's height is equal to the minimum height within the tile, as specified in the tile's header. 
     When the value is 32767, the vertex's height is equal to the maximum height within the tile. For other values, the vertex's height is a linear interpolation between the minimum and maximum heights.
           
     */

    public uint vertexCount;
    public ushort[] u;
    public ushort[] v;
    public ushort[] height;

    public VertexData()
    {
    }

    public VertexData(uint width, uint hgt)
    {
      vertexCount = (width) * (hgt);
      u = new ushort[vertexCount];
      v = new ushort[vertexCount];
      height = new ushort[vertexCount];
      if (vertexCount > 64 * 1024)
        throw new NotSupportedException("32 bit indices not supported yet");
    }

    public void AddVertex(int idx, ushort x, ushort y, ushort z)
    {
      u[idx] = x == 0 ? x : (ushort)(x-1); // allow for zero based range
      v[idx] = y == 0 ? y : (ushort)(y-1);
      height[idx] = z;
    }

    public VertexData(BinaryReader reader)
    {
      vertexCount = reader.ReadUInt32();
      u = new ushort[vertexCount];
      v = new ushort[vertexCount];
      height = new ushort[vertexCount];

      if (vertexCount > 64 * 1024)
        throw new NotSupportedException("32 bit indices not supported yet");

      for (int i = 0; i < vertexCount; i++)
        u[i] = reader.ReadUInt16();

      for (int i = 0; i < vertexCount; i++)
        v[i] = reader.ReadUInt16();

      for (int i = 0; i < vertexCount; i++)
        height[i] = reader.ReadUInt16();

      // now decode deltas and place true value back into array
      ushort _u = 0;
      ushort _v = 0;
      ushort _height = 0;

      for (int i = 0; i < vertexCount; i++)
      {
        _u += (ushort)ZigZag.Decode(u[i]);
        _v += (ushort)ZigZag.Decode(v[i]);
        _height += (ushort)ZigZag.Decode(height[i]);

        u[i] = _u;
        v[i] = _v;
        height[i] = _height;
      }
    }
  }
}

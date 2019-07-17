using System.IO;
using System.Runtime.InteropServices;


namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  /*
  Immediately following the vertex data is the index data. Indices specify how the vertices are linked together into triangles.
  If tile has more than 65536 vertices, the tile uses the IndexData32 structure to encode indices. Otherwise, it uses the IndexData16 structure.
  To enforce proper byte alignment, padding is added before the IndexData to ensure 2 byte alignment for IndexData16 and 4 byte alignment for IndexData32. 
  */

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class IndexData16
  {
    public uint triangleCount;
    public ushort[] indices;


    public IndexData16()
    {
    }

    public IndexData16(BinaryReader reader)
    {
      triangleCount = reader.ReadUInt32();
      indices = new ushort[triangleCount * 3];

      // Indices are encoded using the high water mark encoding from webgl-loader. Indices are decoded as follows:
      ushort highest = 0;
      for (int i = 0; i < indices.Length; i++)
      {
        ushort code = reader.ReadUInt16();
        indices[i] = (ushort)(highest - code);

        if (code == 0)
          highest++;
      }
    }
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class IndexData322
  {
    public uint triangleCount;
    public uint[] indices;
  }


  //Each triplet of indices specifies one triangle to be rendered, in counter-clockwise winding order.Following the triangle indices is four more lists of indices
  //These index lists enumerate the vertices that are on the edges of the tile. It is helpful to know which vertices are on the edges in order to add skirts to hide cracks between adjacent levels of detail.

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class EdgeIndices16
  {
    public uint westVertexCount;
    public ushort[] westIndices;

    public uint southVertexCount;
    public ushort[] southIndices;

    public uint eastVertexCount;
    public ushort[] eastIndices;

    public uint northVertexCount;
    public ushort[] northIndices;

    public EdgeIndices16()
    {
    }

    public EdgeIndices16(BinaryReader reader)
    {
      westVertexCount = reader.ReadUInt32();
      westIndices = new ushort[westVertexCount];

      for (int i = 0; i < westVertexCount; i++)
        westIndices[i] = reader.ReadUInt16();

      southVertexCount = reader.ReadUInt32();
      southIndices = new ushort[southVertexCount];

      for (int i = 0; i < southVertexCount; i++)
        southIndices[i] = reader.ReadUInt16();

      eastVertexCount = reader.ReadUInt32();
      eastIndices = new ushort[eastVertexCount];

      for (int i = 0; i < eastVertexCount; i++)
        eastIndices[i] = reader.ReadUInt16();

      northVertexCount = reader.ReadUInt32();
      northIndices = new ushort[northVertexCount];

      for (int i = 0; i < northVertexCount; i++)
        northIndices[i] = reader.ReadUInt16();
    }
  }
}

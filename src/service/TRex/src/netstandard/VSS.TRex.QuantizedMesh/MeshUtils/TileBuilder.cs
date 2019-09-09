using System;
using System.IO;
namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public class TileBuilder
  {
    private int _TriangleCount;
    private int _GridSize;
    public TerrainTileHeader MeshHeader;
    public VertexData MeshVertexData;
    public IndexData16 MeshIndexData16;
    public EdgeIndices16 MeshEdgeIndices16; // todo seperate out

    private void MakeHeader(TerrainTileHeader headerRec)
    {
      MeshHeader = new TerrainTileHeader()
      {
        CenterX = headerRec.CenterX,
        CenterY = headerRec.CenterY,
        CenterZ = headerRec.CenterZ,
        MinimumHeight = headerRec.MinimumHeight,
        MaximumHeight = headerRec.MaximumHeight,
        BoundingSphereCenterX = headerRec.BoundingSphereCenterX,
        BoundingSphereCenterY = headerRec.BoundingSphereCenterY,
        BoundingSphereCenterZ = headerRec.BoundingSphereCenterZ,
        BoundingSphereRadius = headerRec.BoundingSphereRadius,
        HorizonOcclusionPointX = headerRec.HorizonOcclusionPointX,
        HorizonOcclusionPointY = headerRec.HorizonOcclusionPointY,
        HorizonOcclusionPointZ = headerRec.HorizonOcclusionPointZ
      };

    }

    /// <summary>
    /// This determines the order the triangles are made. Must be anti clockwise
    /// </summary>
    private void MakeTriangleIndices()
    {
      // winding should be anticlockwise
      MeshIndexData16 = new IndexData16()
      {
        triangleCount = (uint)_TriangleCount,
        indices = new ushort[_TriangleCount * 3]
      };

      try
      {

        int m = 0;
        for (int y = 0; y < _GridSize - 1; y++) // bottom row to row just before top
        {
          var r = y * _GridSize;

          for (int x = 0; x < _GridSize - 1; x++)
          {
            // triangle winding order is anti clockwise from SW, NE, NW, SW, SE, NE
            MeshIndexData16.indices[m] = (ushort)(r);
            MeshIndexData16.indices[m + 1] = (ushort)(r + _GridSize + 1);
            MeshIndexData16.indices[m + 2] = (ushort)(r + _GridSize);
            MeshIndexData16.indices[m + 3] = (ushort)(r);
            MeshIndexData16.indices[m + 4] = (ushort)(r + 1);
            MeshIndexData16.indices[m + 5] = (ushort)(r + _GridSize + 1);
            m = m + 6;
            r++;
          }
        }

      }
      catch (Exception exception)
      {
        Console.WriteLine($"Unexpected exception: {exception}");
      }

      // high water mark encoding
      ushort highest = 0;
      ushort code;
      for (int i = 0; i < MeshIndexData16.indices.Length; i++)
      {
        code = (ushort)(highest - MeshIndexData16.indices[i]);
        MeshIndexData16.indices[i] = code;
        if (code == 0)
          highest++;
      }

    }

    private void MakeEdgeIndicesData()
    {
      uint westCount = (uint)_GridSize;
      uint southCount = (uint)_GridSize;
      uint eastCount = (uint)_GridSize;
      uint northCount = (uint)_GridSize;

      MeshEdgeIndices16 = new EdgeIndices16()
      {
        westVertexCount = westCount,
        westIndices = new ushort[westCount],
        eastVertexCount = eastCount,
        eastIndices = new ushort[eastCount],
        northVertexCount = northCount,
        northIndices = new ushort[northCount],
        southVertexCount = southCount,
        southIndices = new ushort[southCount]
      };

      ushort y = 0;
      for (uint i = 0; i < MeshEdgeIndices16.westVertexCount; i++)
      {
        MeshEdgeIndices16.westIndices[i] = y;
        y = (ushort)(y + _GridSize);
      }

      for (uint i = 0; i < MeshEdgeIndices16.southVertexCount; i++)
        MeshEdgeIndices16.southIndices[i] = (ushort)i;

      y = 0;
      for (uint i = 0; i < MeshEdgeIndices16.eastVertexCount; i++)
      {
        MeshEdgeIndices16.eastIndices[i] = (ushort)(y + _GridSize - 1);
        y = (ushort)(y + _GridSize);
      }

      y = (ushort)((_GridSize - 1) * (_GridSize - 1) + _GridSize - 1);

      for (uint i = 0; i < MeshEdgeIndices16.northVertexCount; i++)
        MeshEdgeIndices16.northIndices[i] = (ushort)(y + i);

    }

    public void EncodeVertices(ref VertexData vData)
    {
      // ZigZag Encode Buffer. The quantizing of data I guess
      ushort _u;
      ushort _v;
      ushort _height;
      ushort prev_u = 0;
      ushort prev_v = 0;
      ushort prev_height = 0;

      for (int i = 0; i < vData.u.Length; i++)
      {
        // work out delta of current value minus prev value and encode
        _u = (ushort)ZigZag.Encode(vData.u[i] - prev_u);
        _v = (ushort)ZigZag.Encode(vData.v[i] - prev_v);
        _height = (ushort)ZigZag.Encode(vData.height[i] - prev_height);

        prev_u = vData.u[i];
        prev_v = vData.v[i];
        prev_height = vData.height[i];

        vData.u[i] = _u;
        vData.v[i] = _v;
        vData.height[i] = _height;
      }

      MeshVertexData = vData;
    }

    /// <summary>
    ///  From the vertices passed in make quantized mesh tile
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="vertexNormals"></param>
    /// <param name="headerRec"></param>
    /// <param name="trianglesCount"></param>
    /// <param name="gridSize"></param>
    /// <param name="hasLighting"></param>
    /// <returns>Quantized Mesh Tile</returns>
    public byte[] MakeTile(VertexData vertices, ref byte[] vertexNormals, TerrainTileHeader headerRec, int trianglesCount, int gridSize, bool hasLighting)
    {
      // Assemble tile data 
      _TriangleCount = trianglesCount;
      _GridSize = gridSize;

      MakeHeader(headerRec);

      EncodeVertices(ref vertices);

      MakeTriangleIndices();

      MakeEdgeIndicesData();

      // We are now ready to assemble the tile into a byte array
      var ms = new MemoryStream();
      using (BinaryWriter writer = new BinaryWriter(ms))
      {
        // write header
        writer.Write(MeshHeader.CenterX);
        writer.Write(MeshHeader.CenterY);
        writer.Write(MeshHeader.CenterZ);
        writer.Write(MeshHeader.MinimumHeight);
        writer.Write(MeshHeader.MaximumHeight);
        writer.Write(MeshHeader.BoundingSphereCenterX);
        writer.Write(MeshHeader.BoundingSphereCenterY);
        writer.Write(MeshHeader.BoundingSphereCenterZ);
        writer.Write(MeshHeader.BoundingSphereRadius);
        writer.Write(MeshHeader.HorizonOcclusionPointX);
        writer.Write(MeshHeader.HorizonOcclusionPointY);
        writer.Write(MeshHeader.HorizonOcclusionPointZ);

        // write vertex data in order. count, long array, lat array, height array
        writer.Write(MeshVertexData.vertexCount);
        for (int i = 0; i < MeshVertexData.vertexCount; i++)
          writer.Write(MeshVertexData.u[i]); // longitude
        for (int i = 0; i < MeshVertexData.vertexCount; i++)
          writer.Write(MeshVertexData.v[i]); // latitude
        for (int i = 0; i < MeshVertexData.vertexCount; i++)
          writer.Write(MeshVertexData.height[i]); //heights

        // write triangle indices
        writer.Write(MeshIndexData16.triangleCount);
        for (int i = 0; i < MeshIndexData16.triangleCount * 3; i++) // three indices to a triangle
          writer.Write(MeshIndexData16.indices[i]);

        // write west indices
        writer.Write(MeshEdgeIndices16.westVertexCount);
        for (int i = 0; i < MeshEdgeIndices16.westVertexCount; i++)
          writer.Write(MeshEdgeIndices16.westIndices[i]);

        // write south indices
        writer.Write(MeshEdgeIndices16.southVertexCount);
        for (int i = 0; i < MeshEdgeIndices16.southVertexCount; i++)
          writer.Write(MeshEdgeIndices16.southIndices[i]);

        // write east indices
        writer.Write(MeshEdgeIndices16.eastVertexCount);
        for (int i = 0; i < MeshEdgeIndices16.eastVertexCount; i++)
          writer.Write(MeshEdgeIndices16.eastIndices[i]);

        // write north indices
        writer.Write(MeshEdgeIndices16.northVertexCount);
        for (int i = 0; i < MeshEdgeIndices16.northVertexCount; i++)
          writer.Write(MeshEdgeIndices16.northIndices[i]);

        if (hasLighting)
        { // write normal map
          writer.Write((byte)1);
          writer.Write(vertexNormals.Length);
          for (int i = 0; i < vertexNormals.Length; i++)
            writer.Write((byte)vertexNormals[i]);
        }

        /* if we ever show animated water flow on tile. Watermask
        writer.Write((byte)2);
        writer.Write((int)1);
        //        writer.Write((byte)255); // water
        writer.Write((byte)0); // land
        */

        return ms.ToArray();
      }

    }
  }
}

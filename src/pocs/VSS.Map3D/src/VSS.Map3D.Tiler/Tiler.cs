using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VSS.Map3D.Common;
using VSS.Map3D.Models.QMTile;

/*
 Tiler is a quantized mesh tile generator.
 Each tile is a specially-encoded triangle mesh where vertices overlap their neighbors at tile edges.In other words,
 at the root, the eastern-most vertices in the western tile have the same longitude as the western-most vertices in the eastern tile.
*/


namespace VSS.Map3D.Tiler
{

  public class Tiler : ITiler
  {

    private int _TriangleCount;
    private int _GridSize;
    public TerrainTileHeader MeshHeader;
    public VertexData MeshVertexData;
    public IndexData16 MeshIndexData16;
    public EdgeIndices16 MeshEdgeIndices16;

    /// <summary>
    /// Temp class
    /// </summary>
    /// <param name="minElevation"></param>
    /// <param name="maxElevation"></param>
    private void MakeFakeHeader(float minElevation, float maxElevation)
    {

      // todo fill in all fields correctly
      MeshHeader = new TerrainTileHeader()
      {
        CenterX = -4869750.60295,
        CenterY = 517839.417383868,
        CenterZ = -4005385.98852821,
        MinimumHeight = minElevation,
        MaximumHeight = maxElevation,
        BoundingSphereCenterX = -4869750.60295,
        BoundingSphereCenterY = 517839.417383868,
        BoundingSphereCenterZ = -4005385.98852821,
        BoundingSphereRadius = 832547.176047396,
        HorizonOcclusionPointX = 0.5,
        HorizonOcclusionPointY = 0.5,
        HorizonOcclusionPointZ = 0.5
      };

    }

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
        triangleCount = (uint) _TriangleCount,
        indices = new ushort[_TriangleCount * 3]
      };

      try
      {

        int m = 0;
        for (int y = 0; y < _GridSize-1; y++) // bottom row to row just before top
        {
          // var r = (y * _GridSize) + y;
          var r = y * _GridSize;

          for (int x = 0; x < _GridSize-1; x++) 
          {
            /*            MeshIndexData16.indices[m]   = (ushort)(r);
                        MeshIndexData16.indices[m+1] = (ushort)(r + _GridSize + 2);
                        MeshIndexData16.indices[m+2] = (ushort)(r + _GridSize + 1); 
                        MeshIndexData16.indices[m+3] = (ushort)(r);
                        MeshIndexData16.indices[m+4] = (ushort)(r+1);
                        MeshIndexData16.indices[m+5] = (ushort)(r + _GridSize + 2);
                        */
            // triangle winding order is anti clockwise from SW, NE, NW, SW, SE, NE
            MeshIndexData16.indices[m] = (ushort)(r);
            MeshIndexData16.indices[m + 1] = (ushort)(r + _GridSize + 1);
            MeshIndexData16.indices[m + 2] = (ushort)(r + _GridSize);
            MeshIndexData16.indices[m + 3] = (ushort)(r);
            MeshIndexData16.indices[m + 4] = (ushort)(r + 1);
            MeshIndexData16.indices[m + 5] = (ushort)(r + _GridSize + 1);

            m = m +6;
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
      //todo work out edge vertices
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
        MeshEdgeIndices16.eastIndices[i] = (ushort)(y+_GridSize-1);
        y = (ushort)(y + _GridSize);
      }

      y = (ushort)((_GridSize-1) * (_GridSize-1) + _GridSize -1);
     // y = (ushort) (Math.Pow((_GridSize - 1), 2) + _GridSize - 1);

      for (uint i = 0; i < MeshEdgeIndices16.northVertexCount; i++)
        MeshEdgeIndices16.northIndices[i] = (ushort)(y+i);
       
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
    /// From the vertices passed in make quantized mesh tile
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="headerRec"></param>
    /// <param name="trianglesCount"></param>
    /// <param name="tileSize"></param>
    /// <returns></returns>
    public byte[] MakeTile(VertexData vertices, TerrainTileHeader headerRec, int trianglesCount, int gridSize)
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
      //      using (BinaryWriter writer = new BinaryWriter(File.Open(outFile, FileMode.Create)))
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

        // todo normals

        return ms.ToArray();
      }

    }

    /// <summary>
    /// Returns an existing terrain tile
    /// </summary>
    /// <param name="tileDir">Folder containing static tile </param>
    /// <param name="x">Grid number for longitude</param>
    /// <param name="y">Grid number for latitude</param>
    /// <param name="z">Grid zoom level</param>
    /// <returns></returns>
    public async Task<byte[]> FetchTile(string tileDir, int x, int y, int z)
    {
      // todo go get precompiled tile from disk or cache

      var fileInfo = new FileInfo(Path.Combine(tileDir, string.Format(@"{0}\{1}\{2}.terrain", z, x, y)));
      if (fileInfo.Exists)
      {
        var buffer = new byte[fileInfo.Length];
        using (var fileStream = fileInfo.OpenRead())
        {
          await fileStream.ReadAsync(buffer, 0, buffer.Length);
          Console.WriteLine("Tile {0} sent", fileInfo);
          return buffer.ToArray();
        }
      }
      Console.WriteLine("*** Tile {0} was NOT sent ***", fileInfo);
      return null;
    }

    /// <summary>
    /// Returns a dynamic tile based on TMS XYZ tile coordinates
    /// </summary>
    /// <param name="options">Options to generate tile</param>
    /// <param name="x">Grid number for longitude</param>
    /// <param name="y">Grid number for latitude</param>
    /// <param name="z">Grid zoom level</param>
    /// <returns></returns>
    public Task<byte[]> GetXYZTile(TileOptions options, int x, int y, int z)
    {
      // todo See terrain controller to implement
      // Fetch DEM
      // Make Mesh
      // Call MakeTile
      throw new NotImplementedException();
    }
  }
}

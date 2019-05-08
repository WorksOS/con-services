using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using VSS.Map3D.Models;
using VSS.Map3D.Models.QMTile;

namespace VSS.Map3D.Quantize
{
  public class QuantizeDEM
  {

    public uint vertexCount;

//    public ushort[] u;
//    public ushort[] v;
    //   public ushort[] height;
    public TerrainTileHeader Header;
    public VertexData VertexData;
    public IndexData16 IndexData16;
    public EdgeIndices16 EdgeIndices16;

    public QuantizeDEM()
    {
    }

    /// <summary>
    /// Make tile header record
    /// </summary>
    private void MakeHeader(ref ElevationData evData)
    {

      // todo fill in all fields correctly
      Header = new TerrainTileHeader()
      {
        CenterX = -4869750.60295,
        CenterY = 517839.417383868,
        CenterZ = -4005385.98852821,
        MinimumHeight = evData.MinElevation,
        MaximumHeight = evData.MaxElevation,
        BoundingSphereCenterX = -4869750.60295,
        BoundingSphereCenterY = 517839.417383868,
        BoundingSphereCenterZ = -4005385.98852821,
        BoundingSphereRadius = 832547.176047396,
        HorizonOcclusionPointX = -0.775337351286795,
        HorizonOcclusionPointY = 0.0824478037997883,
        HorizonOcclusionPointZ = -0.639862876673674
      };

    }

    private void MakeVertexData(int GridSize, ref VertexData verts)
    {

      if (vertexCount > 64 * 1024)
        throw new NotSupportedException("32 bit indices not supported yet");

      uint numVerts = (uint) (GridSize * GridSize);

      VertexData = verts;
      vertexCount = numVerts;

      /*

      VertexData = new VertexData()
      {
        vertexCount = numVerts,
        u = new ushort[numVerts],
        v = new ushort[numVerts],
        height = new ushort[numVerts]
      };

      for (int i = 1; i < GridSize; i++)
      {
        for (int j = 0; j < GridSize; j++)
        {

          VertexData.u[i] = verts[i*GridSize].x;
          VertexData.v[0] = verts[i * GridSize].y; ; 
          VertexData.height[0] = verts[i * GridSize].z;

        }
      }
      */

      // now zigzag encode
      // now decode deltas and place true value back into array
      /*
      ushort _u = 0;
      ushort _v = 0;
      ushort _height = 0;

      for (int i = 0; i < vertexCount; i++)
      {
        _u += (ushort)ZigZag.Encode(u[i]);
        _v += (ushort)ZigZag.Encode(v[i]);
        _height += (ushort)ZigZag.Encode(height[i]);

        u[i] = _u;
        v[i] = _v;
        height[i] = _height;
      }
      */
    }

    private void MakeVertexDataTest()
    {

      if (vertexCount > 64 * 1024)
        throw new NotSupportedException("32 bit indices not supported yet");

      uint numVerts = 4;


      VertexData = new VertexData()
      {
        vertexCount = numVerts,
        u = new ushort[numVerts],
        v = new ushort[numVerts],
        height = new ushort[numVerts]
      };

      // Anti clockwise
      // bottom left
      VertexData.u[0] = 0; // x or lon
      VertexData.v[0] = 0; // y or lat
      VertexData.height[0] = 16384; // max height

      // top left
      VertexData.u[1] = 0; // x or lon
      VertexData.v[1] = 32767; // y or lat
      VertexData.height[1] = 0;

      // right bott
      VertexData.u[2] = 32767; // x or lon
      VertexData.v[2] = 0; // y or lat
      VertexData.height[2] = 32767;

      // top right
      VertexData.u[3] = 32767; // x or lon
      VertexData.v[3] = 32767; // y or lat
      VertexData.height[3] = 16384;

      // now zigzag encode
      // now decode deltas and place true value back into array
      /*
      ushort _u = 0;
      ushort _v = 0;
      ushort _height = 0;
      //  Mathf.Lerp();

      for (int i = 0; i < vertexCount; i++)
      {
        _u += (ushort)ZigZag.Encode(u[i]);
        _v += (ushort)ZigZag.Encode(v[i]);
        _height += (ushort)ZigZag.Encode(height[i]);

        u[i] = _u;
        v[i] = _v;
        height[i] = _height;
      }
      */
    }

    public void EncodeVertexBuffer()
    {
      // ZigZag Encode Buffer. The quantizing of data I guess
      ushort _u;
      ushort _v;
      ushort _height;
      ushort prev_u = 0;
      ushort prev_v = 0;
      ushort prev_height = 0;

      for (int i = 0; i < VertexData.u.Length; i++)
      {
        // work out delta of current value minus prev value and encode
        _u = (ushort) ZigZag.Encode(VertexData.u[i] - prev_u);
        _v = (ushort) ZigZag.Encode(VertexData.v[i] - prev_v);
        _height = (ushort) ZigZag.Encode(VertexData.height[i] - prev_height);

        prev_u = VertexData.u[i];
        prev_v = VertexData.v[i];
        prev_height = VertexData.height[i];

        VertexData.u[i] = _u;
        VertexData.v[i] = _v;
        VertexData.height[i] = _height;
      }
    }


    private void MakeIndexData()
    {
      IndexData16 = new IndexData16()
      {
        triangleCount = 2,
        indices = new ushort[2 * 3]
      };

      /*
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


  */
      // triangles to draw anti clockwise
      IndexData16.indices[0] = 0;
      IndexData16.indices[1] = 3;
      IndexData16.indices[2] = 1;
      IndexData16.indices[3] = 0;
      IndexData16.indices[4] = 2;
      IndexData16.indices[5] = 3;
    }


    private void MakeEdgeIndicesData()
    {
      EdgeIndices16 = new EdgeIndices16()
      {
        westVertexCount = 2,
        westIndices = new ushort[2],
        eastVertexCount = 2,
        eastIndices = new ushort[2],
        northVertexCount = 2,
        northIndices = new ushort[2],
        southVertexCount = 2,
        southIndices = new ushort[2]
      };

      /*
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

  */

      // vertexs touching edge
      EdgeIndices16.westIndices[0] = 0;
      IndexData16.indices[1] = 1;
      EdgeIndices16.southIndices[0] = 0;
      IndexData16.indices[1] = 2;
      EdgeIndices16.eastIndices[0] = 2;
      IndexData16.indices[1] = 3;
      EdgeIndices16.northIndices[0] = 1;
      IndexData16.indices[1] = 3;


      /*
      // now decode deltas and place true value back into array
      ushort _u = 0;
      ushort _v = 0;
      ushort _height = 0;

      for (int i = 0; i < vertexCount; i++)
      {
        _u += (ushort) ZigZag.Decode(u[i]);
        _v += (ushort) ZigZag.Decode(v[i]);
        _height += (ushort) ZigZag.Decode(height[i]);

        u[i] = _u;
        v[i] = _v;
        height[i] = _height;
      }       
       *
       */


      // Now encode data in CPP
      /*
        // # Write mesh vertices (X Y Z components of each vertex):
        int vertexCount = mMesh.vertices.size();
        ostream.write(&vertexCount, sizeof(int));

        for (int c = 0; c < 3; c++) {
          double origin = bounds.min[c];
          double factor = 0;
          if (bounds.max[c] > bounds.min[c]) factor = SHORT_MAX / (bounds.max[c] - bounds.min[c]);

          // Move the initial value
          int u0 = quantizeIndices(origin, factor, mMesh.vertices[0][c]), u1, ud;
          uint16_t sval = zigZagEncode(u0);
          ostream.write(&sval, sizeof(uint16_t));

          for (size_t i = 1, icount = mMesh.vertices.size(); i < icount; i++) {
            u1 = quantizeIndices(origin, factor, mMesh.vertices[i][c]);
            ud = u1 - u0;
            sval = zigZagEncode(ud);
            ostream.write(&sval, sizeof(uint16_t));
            u0 = u1;
          }
        } 
       */










    }

    public void EncodeVertexBuffer(VertexData vData)
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
        _u = (ushort) ZigZag.Encode(vData.u[i] - prev_u);
        _v = (ushort) ZigZag.Encode(vData.v[i] - prev_v);
        _height = (ushort) ZigZag.Encode(vData.height[i] - prev_height);

        prev_u = vData.u[i];
        prev_v = vData.v[i];
        prev_height = vData.height[i];

        vData.u[i] = _u;
        vData.v[i] = _v;
        vData.height[i] = _height;
      }
    }

    public void EncodeIndices(IndexData16 idxData)
    {

      /*
       *
             So you can also get a pretty big win by exploiting pre-transform vertex cache optimization.
             I call it “high water mark encoding.” Basically: for such a properly optimized index list,
             the next index you see is either (a) one you’ve seen before or (b) one higher than the current highest seen index.
             So, instead of encoding actual indices, you can instead encode them relative to this high water mark (the largest index yet to be seen, initialized to 0).
             You see “n” and that corresponds to an index of (high water mark – n). When you see 0, you also increment high watermark.       
             The benefit here is that the encoded indices are very small, and then you can do some kind of varint coding,
             then your encoded indices are a bit more than a byte on average. If you plan on zipping later, then make sure the variants are byte-aligned and LSB-first.

    uint16_t highest = 0;
    uint16_t code;

    // Write main indices
    for (size_t i = 0, icount = mMesh.indices.size(); i < icount; i++) {
      code = highest - mMesh.indices[i];
      ostream.write(&code, sizeof(uint16_t));
      if (code == 0) highest++;
    }

    // Write all vertices on the edge of the tile (W, S, E, N)
    writeEdgeIndices<uint16_t>(ostream, mMesh, bounds.min.x, 0);
    writeEdgeIndices<uint16_t>(ostream, mMesh, bounds.min.y, 1);
    writeEdgeIndices<uint16_t>(ostream, mMesh, bounds.max.x, 0);
    writeEdgeIndices<uint16_t>(ostream, mMesh, bounds.max.y, 1);
  }


       */

      ushort highest = 0;
      ushort code;

      for (int i = 0; i < idxData.indices.Length; i++)
      {
        code = (ushort) (highest - idxData.indices[i]);
        idxData.indices[i] = code;
        if (code == 0)
          highest++;
      }

      /*
      triangleCount = reader.ReadUInt32();
      indices = new ushort[triangleCount * 3];

      // Indices are encoded using the high water mark encoding from webgl-loader. Indices are decoded as follows:
      ushort highest = 0;
      for (int i = 0; i<indices.Length; i++)
      {
        ushort code = reader.ReadUInt16();
        indices[i] = (ushort) (highest - code);

        if (code == 0)
          highest++;
      }
      */
    }



    public async Task<byte[]> QuantizeDEMAsync(ElevationData ed)
    {
      //  throw new NotImplementedException();

      VertexData verts = QuantizeMesh.MakeRegularQMesh(ref ed);

      // Assemble tile data 
      MakeHeader(ref ed);
      MakeVertexData(ed.TileSize, ref verts);
      MakeIndexData();
      MakeEdgeIndicesData();


      //   var newTerrain = new TerrainTile();
      var ms = new MemoryStream();
      using (BinaryWriter writer = new BinaryWriter(ms))
        //      using (BinaryWriter writer = new BinaryWriter(File.Open(outFile, FileMode.Create)))
      {
        // write header
        writer.Write(Header.CenterX);
        writer.Write(Header.CenterY);
        writer.Write(Header.CenterZ);
        writer.Write(Header.MinimumHeight);
        writer.Write(Header.MaximumHeight);
        writer.Write(Header.BoundingSphereCenterX);
        writer.Write(Header.BoundingSphereCenterY);
        writer.Write(Header.BoundingSphereCenterZ);
        writer.Write(Header.BoundingSphereRadius);
        writer.Write(Header.HorizonOcclusionPointX);
        writer.Write(Header.HorizonOcclusionPointY);
        writer.Write(Header.HorizonOcclusionPointZ);


        // write vertex data
        EncodeVertexBuffer(VertexData); // quantize
        writer.Write(VertexData.vertexCount);
        for (int i = 0; i < VertexData.vertexCount; i++)
          writer.Write(VertexData.u[i]); // longitude
        for (int i = 0; i < VertexData.vertexCount; i++)
          writer.Write(VertexData.v[i]); // latitude
        for (int i = 0; i < VertexData.vertexCount; i++)
          writer.Write(VertexData.height[i]); //heights


        // write triangle indices
        EncodeIndices(IndexData16);
        writer.Write(IndexData16.triangleCount);
        for (int i = 0; i < IndexData16.triangleCount * 3; i++)
          writer.Write(IndexData16.indices[i]);

        // write west indices
        writer.Write(EdgeIndices16.westVertexCount);
        for (int i = 0; i < EdgeIndices16.westVertexCount; i++)
          writer.Write(EdgeIndices16.westIndices[i]);

        // write south indices
        writer.Write(EdgeIndices16.southVertexCount);
        for (int i = 0; i < EdgeIndices16.southVertexCount; i++)
          writer.Write(EdgeIndices16.southIndices[i]);

        // write east indices
        writer.Write(EdgeIndices16.eastVertexCount);
        for (int i = 0; i < EdgeIndices16.eastVertexCount; i++)
          writer.Write(EdgeIndices16.eastIndices[i]);

        // write north indices
        writer.Write(EdgeIndices16.northVertexCount);
        for (int i = 0; i < EdgeIndices16.northVertexCount; i++)
          writer.Write(EdgeIndices16.northIndices[i]);

        // todo normals

        // var bytes = File.ReadAllBytes(ms);


//        using (FileStream fs = new FileStream(outFile, FileMode.Create))
        //       using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
        //      {
        //       zipStream.Write(ms.ToArray(), 0, ms.ToArray().Length);// .Write(bytes, 0, bytes.Length);
        //    }


        byte[] buffer = ms.ToArray(); // new byte[100];

        return buffer;
      }
    }
  }
}

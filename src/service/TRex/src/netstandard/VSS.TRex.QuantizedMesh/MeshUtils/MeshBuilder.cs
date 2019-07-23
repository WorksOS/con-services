using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public static class MeshBuilder
  {


    public static void ComputeHeaderInfo(ref ElevationData evlData)
    {
      int m = 0;
      for (int y = 0; y < evlData.GridSize; y++)
      {
        for (int x = 0; x < evlData.GridSize; x++)
        {
          //   vertices.AddVertex(m, (ushort)(x * ratio), (ushort)(y * ratio), QuantizeHeight(evlData.MinElevation, evlData.MaxElevation, evlData.Elev[m]));
          m++;
        }
      }

    }


    public static ushort QuantizeHeight(float origStart, float origEnd, float value)
    {
      if (value >= origEnd)
        return 32767; // max tile height
      if (value <= origStart)
        return 0; // min tile height
      // new scale is zero based
      return (ushort)((value - origStart) * ((double)32767 / (origEnd - origStart)));
    }



    public static VertexData MakeQuantizedMesh(ref ElevationData evlData)
    {


      int ratio = 32767 / evlData.GridSize;
      VertexData vertices = new VertexData((uint)evlData.GridSize, (uint)evlData.GridSize);
      // data flows sw to se and up
      int m = 0;
      //    bool firstRow = true;
//      for (int y = 0; y <= evlData.GridSize; y++)
      for (int y = 0; y <= evlData.GridSize-1; y++)
      {
//        for (int x = 0; x <= evlData.GridSize; x++)
        for (int x = 0; x <= evlData.GridSize-1; x++)
        {
            vertices.AddVertex(m, (ushort)(x * ratio), (ushort)(y * ratio), QuantizeHeight(evlData.MinimumHeight, evlData.MaximumHeight, evlData.Elev[m]));
          m++;
        }
      }
      return vertices;
    }

    public static VertexData MakeFakeMesh(ref ElevationData evlData)
    {
      // Simple routine to convert or grid of float elevations into a format suitable for a quantized mesh of unsigned short ints

      VertexData vertices = new VertexData((uint)evlData.GridSize, (uint)evlData.GridSize);
      try
      {

        double ratio = 32767 / (double)(evlData.GridSize - 1);

        // data flows sw to se and up
        int m = 0;
        for (int y = 0; y < evlData.GridSize; y++)
        {
          for (int x = 0; x < evlData.GridSize; x++)
          {
            try
            {
              // todo ratio is wrong
              vertices.AddVertex(m, (ushort)(x * ratio), (ushort)(y * ratio),
                QuantizeHeight(evlData.MinimumHeight, evlData.MaximumHeight, evlData.Elev[m]));
            }
            catch (Exception ex1)
            {
              System.Diagnostics.Debug.WriteLine($"**** MakeFakeMesh Error *********:{ex1}");
            }
            m++;
          }
        }
      }
      catch (Exception ex)
      {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"**** MakeFakeMesh Error *********:{ex}");
#endif
      }

      return vertices;
    }
  }
}

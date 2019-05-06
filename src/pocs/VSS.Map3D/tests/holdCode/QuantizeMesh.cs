using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VSS.Map3D.Models;
using VSS.Map3D.Models.QMTile;

namespace VSS.Map3D.Quantize
{
  public class QuantizeMesh
  {

    public static ushort ConvertRange(double min, double max, double value) // value to convert
    {
      //double scale = (double)(32768) / (max - min);
      return (ushort)((value - min) * ((double)(32768) / (max - min)));
    }

    public static Vertices3[] MakeRegularMesh(ref ElevationData evData)
    {
      evData.Elev[0] = 1;
      int ratio = 32768 / evData.TileSize;
    //  min = double.MaxValue;
   //   max = double.MinValue;
      double fact = (evData.MaxElevation - evData.MinElevation) / 32768;
      Vertices3[] vertices = new Vertices3[evData.TileSize * evData.TileSize];

      for (int i = 0; i < evData.TileSize; i++)
      {
        for (int j = 0; j < evData.TileSize; j++)
        {
          var pos = j + (i * evData.TileSize);
          var elv = evData.Elev[pos];
     //     if (elv < min)
      //      min = elv;
       //   if (elv > max)
        //    max = elv;
          vertices[i] = new Vertices3((ushort)(32767 -(i * ratio)), (ushort)(32767 - (j * ratio)), ConvertRange(evData.MinElevation, evData.MaxElevation,elv));
        }
        
      }

      return vertices;

    }

    public static VertexData MakeRegularQMesh(ref ElevationData evData)
    {
      evData.Elev[0] = 1;
      int ratio = 32768 / evData.TileSize;
      double fact = (evData.MaxElevation - evData.MinElevation) / 32768;
      VertexData vertices = new VertexData((uint)evData.TileSize, (uint)evData.TileSize);

      for (int i = 0; i < evData.TileSize; i++)
      {
        for (int j = 0; j < evData.TileSize; j++)
        {
          var pos = j + (i * evData.TileSize);
          var elv = evData.Elev[pos];
          //     if (elv < min)
          //      min = elv;
          //   if (elv > max)
          //    max = elv;
          vertices.AddVertex(pos,(ushort)(32767 - (i * ratio)), (ushort)(32767 - (j * ratio)), ConvertRange(evData.MinElevation, evData.MaxElevation, elv));
        }

      }

      return vertices;

    }

  }
}

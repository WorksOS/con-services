using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Map3D.Models;

namespace MapClient
{
  public class BitmapUtils
  {
    /*
    public float[,] Heightmap = null;

    public void LoadHeightmap(string file, Vector3 scale)

    {

      HeightMapScale = scale;

      Bitmap map = Bitmap.FromFile(file) as Bitmap;

      Width = map.Width;

      Depth = map.Height;

      Heightmap = new float[Width, Depth];

      for (int x = 0; x < Width; x++)

      {

        for (int z = 0; z < Depth; z++)

        {

          Heightmap[x, z] = map.GetPixel(x, z).GetBrightness();

        }

      }

      Console.WriteLine("loadet and created heightmap");

      //NormalizeTerrain();

      //CreateVertices();

      //Console.WriteLine("created vertex");

    }

    private void CreateVertices()

    {

      NumberOfVertices = Width * Depth;

      Vertices = new EvoTerrainVertex[NumberOfVertices];

      for (int x = 0; x < Width; x++)

      {

        for (int z = 0; z < Depth; z++)

        {

          Vertices[z + x * Depth].Postion = new Vector3(x * HeightMapScale.X, Heightmap[x, z] * HeightMapScale.Y, z * HeightMapScale.Z);

          Vertices[z + x * Depth].Normal = new Vector3(0.0f, 1.0f, 0.0f);

          Vertices[z + x * Depth].TextureMap_1 = new Vector2(1.0f / Width * x, 1.0f / Depth * z);

          Vertices[z + x * Depth].TextureMap_2 = new Vector2(1.0f / Width * x, 1.0f / Depth * z);

        }

      }

    }
*/
  }
}

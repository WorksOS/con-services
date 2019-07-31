using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public static class MeshBuilder
  {

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

      // This ratio is an integer and represents the grids intervals in a range from 0 to 32767
      // GridSize should always be an odd number for best result
      int ratio = QMConstants.TileValueRange / (evlData.GridSize-1);

      VertexData vertices = new VertexData((uint)evlData.GridSize, (uint)evlData.GridSize);
      // data flows sw to se and up
      int m = 0;
      for (int y = 0; y < evlData.GridSize; y++)
      {
        for (int x = 0; x < evlData.GridSize; x++)
        {
            vertices.AddVertex(m, (ushort)(x*ratio), (ushort)(y*ratio), QuantizeHeight(evlData.MinimumHeight, evlData.MaximumHeight, evlData.ElevGrid[m]));
          m++;
        }
      }
      return vertices;
    }

  }
}

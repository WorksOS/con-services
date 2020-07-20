using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreX.Interfaces;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Geometry;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public static class AlignmentMasterGeometryHelper
  {
    /// <summary>
    /// Takes the response computed for the alignment, extracts all coordinates into a single list,
    /// converts all coordinates with a call to the coordinate conversion service and inserts the
    /// modified coordinates into the result.
    /// </summary>
    public static void ConvertNEEToLLHCoords(string csib, AlignmentDesignGeometryResponse geometryResponse)
    {
      var coords = new List<XYZ>();
      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Vertices.SelectMany(x => x.Select(x => new XYZ(x[1], x[0], x[2])).ToArray()).ToList());
      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Arcs.SelectMany(x => new[] { new XYZ(x.Y1, x.X1, x.Z1), new XYZ(x.Y2, x.X2, x.Z2), new XYZ(x.YC, x.XC, x.ZC) }).ToList());
      if ((geometryResponse.Labels?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Labels.Select(x => new XYZ(x.Y, x.X, 0.0)).ToList());

      var convertedCoords = DIContext.Obtain<IConvertCoordinates>()
        .NEEToLLH(csib, coords.ToArray().ToCoreX_XYZ(), CoreX.Types.ReturnAs.Degrees)
        .ToTRex_XYZ();

      // Copy the converted coordinates to the geometry response ready for inclusion in the request result
      var index = 0;

      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Vertices.Length; i++)
        {
          for (var j = 0; j < geometryResponse.Vertices[i].Length; j++)
          {
            geometryResponse.Vertices[i][j][0] = convertedCoords[index].X;
            geometryResponse.Vertices[i][j][1] = convertedCoords[index].Y;
            index++;
          }
        }
      }

      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Arcs.Length; i++)
        {
          geometryResponse.Arcs[i].X1 = convertedCoords[index].X;
          geometryResponse.Arcs[i].Y1 = convertedCoords[index].Y;
          geometryResponse.Arcs[i].Z1 = convertedCoords[index].Z;
          index++;
          geometryResponse.Arcs[i].X2 = convertedCoords[index].X;
          geometryResponse.Arcs[i].Y2 = convertedCoords[index].Y;
          geometryResponse.Arcs[i].Z2 = convertedCoords[index].Z;
          index++;
          geometryResponse.Arcs[i].XC = convertedCoords[index].X;
          geometryResponse.Arcs[i].YC = convertedCoords[index].Y;
          geometryResponse.Arcs[i].ZC = convertedCoords[index].Z;
          index++;
        }
      }

      if ((geometryResponse.Labels?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Labels.Length; i++)
        {
          geometryResponse.Labels[i].X = convertedCoords[index].X;
          geometryResponse.Labels[i].Y = convertedCoords[index].Y;
          index++;
        }
      }


    }
  }
}

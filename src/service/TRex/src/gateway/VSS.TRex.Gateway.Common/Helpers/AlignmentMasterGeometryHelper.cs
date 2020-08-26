using System.Collections.Generic;
using System.Linq;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Geometry;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public static class AlignmentMasterGeometryHelper
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger("AlignmentMasterGeometryHelper");

    /// <summary>
    /// Takes the response computed for the alignment, extracts all coordinates into a single list,
    /// converts all coordinates with a call to the coordinate conversion service and inserts the
    /// modified coordinates into the result.
    /// </summary>
    public static void ConvertNEEToLLHCoords(string csib, AlignmentDesignGeometryResponse geometryResponse)
    {
      var coords = new List<XYZ>();

      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Vertices.SelectMany(x => x.Select(pt => new XYZ(x: pt[0], y: pt[1], z: 0.0))));

      if ((geometryResponse.Arcs?.Length ?? 0) > 0)
      {
        coords.AddRange(geometryResponse.Arcs.SelectMany(x =>
        new[]
        {
          new XYZ(x: x.X1, y: x.Y1, z: 0.0),
          new XYZ(x: x.X2, y: x.Y2, z: 0.0),
          new XYZ(x: x.XC, y: x.YC, z: 0.0)
          }
        ));
      }

      if ((geometryResponse.Labels?.Length ?? 0) > 0)
        coords.AddRange(geometryResponse.Labels.Select(x => new XYZ(x: x.X, y: x.Y, z: 0.0)));

      _log.LogDebug($"Assembled vertex & label coordinates before conversion to lat/lon: {string.Join(", ", coords)}");

      var convertedCoords = DIContext.Obtain<ICoreXWrapper>()
        .NEEToLLH(csib, coords.ToArray().ToCoreX_XYZ(), CoreX.Types.ReturnAs.Degrees)
        .ToTRex_XYZ();

      _log.LogDebug($"Assembled vertex & label coordinates after conversion to lat/lon: {string.Join(", ", convertedCoords)}");

      // Copy the converted coordinates to the geometry response ready for inclusion in the request result
      var index = 0;

      if ((geometryResponse.Vertices?.Length ?? 0) > 0)
      {
        for (var i = 0; i < geometryResponse.Vertices.Length; i++)
        {
          for (var j = 0; j < geometryResponse.Vertices[i].Length; j++)
          {
            geometryResponse.Vertices[i][j] = new[] { convertedCoords[index].X, convertedCoords[index].Y, convertedCoords[index].Z };
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

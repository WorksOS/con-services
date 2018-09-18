using System;
using System.Globalization;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Tile.Service.Common.Interfaces;

namespace VSS.Tile.Service.Common.Helpers
{
  /// <summary>
  /// Bounding box helper methods.
  /// </summary>
  public class BoundingBoxHelper : IBoundingBoxHelper
  {
    /// <summary>
    /// Get the bounding box values from the query parameter
    /// </summary>
    /// <param name="bbox">The query parameter containing the bounding box in decimal degrees</param>
    /// <returns>Bounding box in radians</returns>
    public BoundingBox2DLatLon GetBoundingBox(string bbox)
    {
      double blLong = 0;
      double blLat = 0;
      double trLong = 0;
      double trLat = 0;

      var count = 0;
      foreach (var s in bbox.Split(','))
      {
        if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid bounding box"));
        }
        num = num * Math.PI / 180.0; //convert decimal degrees to radians
        //Latitude Must be in range -pi/2 to pi/2 and longitude in the range -pi to pi
        if (count == 0 || count == 2)
        {
          if (num < -Math.PI / 2)
          {
            num = num + Math.PI;
          } else if (num > Math.PI / 2)
          {
            num = num - Math.PI;
          }
        }
        if (count == 1 || count == 3)
        {
          if (num < -Math.PI)
          {
            num = num + 2 * Math.PI;
          } else if (num > Math.PI)
          {
            num = num - 2 * Math.PI;
          }
        }

        switch (count++)
        {
          case 0:
            blLat = num;
            break;
          case 1:
            blLong = num;
            break;
          case 2:
            trLat = num;
            break;
          case 3:
            trLong = num;
            break;
        }
      }

      return new BoundingBox2DLatLon(blLong, blLat, trLong, trLat);
    }
  }
}

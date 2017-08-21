using System;
using VLPDDecls;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public class ProfilesHelper
  {
    public static bool CellGapExists(Velociraptor.PDSInterface.ProfileCell prevCell, Velociraptor.PDSInterface.ProfileCell currCell, out double prevStationIntercept)
    {
      prevStationIntercept = prevCell == null
        ? 0.0
        : prevCell.station + prevCell.interceptLength;

      return prevCell != null && Math.Abs(currCell.station - prevStationIntercept) > 0.001;
    }

    public static void ConvertProfileEndPositions(ProfileGridPoints gridPoints, ProfileLLPoints lLPoints,
                                                 out TWGS84Point startPt, out TWGS84Point endPt,
                                                 out bool positionsAreGrid)
    {
      if (gridPoints != null)
      {
        positionsAreGrid = true;
        startPt = new TWGS84Point { Lat = gridPoints.y1, Lon = gridPoints.x1 };
        endPt = new TWGS84Point { Lat = gridPoints.y2, Lon = gridPoints.x2 };
      }
      else
      if (lLPoints != null)
      {
        positionsAreGrid = false;
        startPt = new TWGS84Point { Lat = lLPoints.lat1, Lon = lLPoints.lon1 };
        endPt = new TWGS84Point { Lat = lLPoints.lat2, Lon = lLPoints.lon2 };
      }
      else
      {
        startPt = new TWGS84Point();
        endPt = new TWGS84Point();
        positionsAreGrid = false;

        // TODO throw an exception
      }
    }
  }
}
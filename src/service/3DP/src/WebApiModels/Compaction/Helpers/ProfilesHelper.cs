using System;
using VLPDDecls;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class ProfilesHelper
  {
    public const int PROFILE_TYPE_NOT_REQUIRED = -1;
    public const int PROFILE_TYPE_HEIGHT = 2;
    public const double ONE_MM = 0.001;

    public static bool CellGapExists(Velociraptor.PDSInterface.ProfileCell prevCell, Velociraptor.PDSInterface.ProfileCell currCell, out double prevStationIntercept)
    {
      return CellGapExists(prevCell?.station, prevCell?.interceptLength, currCell.station, out prevStationIntercept);
    }

    public static bool CellGapExists(Velociraptor.PDSInterface.SummaryVolumesProfileCell prevCell, Velociraptor.PDSInterface.SummaryVolumesProfileCell currCell, out double prevStationIntercept)
    {
      return CellGapExists(prevCell?.station, prevCell?.interceptLength, currCell.station, out prevStationIntercept);
    }

    private static bool CellGapExists(double? prevStation, double? prevInterceptLength, double currStation, out double prevStationIntercept)
    {
      bool hasPrev = prevStation.HasValue && prevInterceptLength.HasValue;
      prevStationIntercept = hasPrev ? prevStation.Value + prevInterceptLength.Value : 0.0;
     
      return hasPrev && Math.Abs(currStation - prevStationIntercept) > ONE_MM;
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

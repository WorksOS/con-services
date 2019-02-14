using System;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class ProfilesHelper
  {
    public const int PROFILE_TYPE_NOT_REQUIRED = -1;
    public const int PROFILE_TYPE_HEIGHT = 2;
    public const double ONE_MM = 0.001;
#if RAPTOR
    public static bool CellGapExists(ProfileCellData prevCell, ProfileCellData currCell, out double prevStationIntercept)
    {
      return CellGapExists(prevCell?.Station, prevCell?.InterceptLength, currCell.Station, out prevStationIntercept);
    }

    public static bool CellGapExists(SummaryVolumesProfileCell prevCell, SummaryVolumesProfileCell currCell, out double prevStationIntercept)
    {
      return CellGapExists(prevCell?.Station, prevCell?.InterceptLength, currCell.Station, out prevStationIntercept);
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
#endif
  }
}

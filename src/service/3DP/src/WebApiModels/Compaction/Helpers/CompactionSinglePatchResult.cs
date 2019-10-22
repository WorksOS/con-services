using System;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public class CompactionSinglePatchResult
  {
    public double elevation = -1;
    public DateTime dateTime = DateTime.MinValue;
    public double easting = -1;
    public double northing = -1;

    public CompactionSinglePatchResult()
    { }

    public CompactionSinglePatchResult(double easting, double northing, double elevation, DateTime dateTime)
    {
      this.easting = easting;
      this.northing = northing;
      this.elevation = elevation;
      this.dateTime = dateTime;
    }

    public CompactionSinglePatchResult[,] UnpackSubgrid(double cellSize, PatchSubgridOriginProtobufResult subgrid)
    {
      var result = new CompactionSinglePatchResult[32, 32];
      var subGridIterator = 0;

      for (int x = 0; x < 32; x++)
      {
        for (int y = 0; y < 32; y++)
        {
          if (subgrid.ElevationOffsets[subGridIterator] > 0)
          {
            result[x, y] = new CompactionSinglePatchResult(
              Math.Round(((subgrid.SubgridOriginX + (cellSize / 2)) + (cellSize * x)), 5),
              Math.Round(((subgrid.SubgridOriginY + (cellSize / 2)) + (cellSize * y)), 5),
              // elevation offsets are in mm
              Math.Round((subgrid.ElevationOrigin + (subgrid.ElevationOffsets[subGridIterator] - 1) / 1000.0), 3),
              (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(subgrid.TimeOrigin).AddSeconds(subgrid.TimeOffsets[subGridIterator])
              );
          }
          subGridIterator++;
        }
      }
      return result;
    }
  }

}

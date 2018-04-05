using System;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class CompactionCellDatumResult : CellDatumResponse
  {
    /// <summary>
    /// The Northing coordinate value of the cell.
    /// </summary>
    public double northing { get; private set; }

    /// <summary>
    /// The Easting coordinate value of the cell.
    /// </summary>
    public double easting { get; private set; }

    /// <summary>
    /// Create an instance of CompactionCellDatumResult class
    /// </summary>
    public static CompactionCellDatumResult CreateCompactionCellDatumResult(
      DisplayMode displayMode,
      short returnCode,
      double? value,
      DateTime timestamp,
      double northing,
      double easting)
    {
      return new CompactionCellDatumResult()
      {
        displayMode = displayMode,
        returnCode = returnCode,
        value = value,
        timestamp = timestamp,
        northing = northing,
        easting = easting
      };
    }
  }
}

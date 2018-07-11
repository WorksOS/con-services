using System;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;

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
    public CompactionCellDatumResult(DisplayMode displayMode, short returnCode, double? value, DateTime timestamp, double northing, double easting) : base(displayMode, returnCode, value, timestamp)
    {
      this.northing = northing;
      this.easting = easting;
    }
  }
}

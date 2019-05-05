using System;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class CompactionCellDatumResult : CellDatumResult
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
    /// Default private constructor.
    /// </summary>
    private CompactionCellDatumResult()
    { }

    /// <summary>
    /// Create an instance of CompactionCellDatumResult class
    /// </summary>
    public CompactionCellDatumResult(DisplayMode displayMode, CellDatumReturnCode returnCode, double? value, DateTime? timestamp, double northing, double easting) 
      : base(displayMode, returnCode, value, timestamp)
    {
      this.northing = northing;
      this.easting = easting;
    }
  }
}

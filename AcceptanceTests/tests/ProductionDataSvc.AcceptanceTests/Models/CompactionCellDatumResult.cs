using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionCellDatumResult : CellDatumResult
  {
    /// <summary>
    /// The Northing coordinate value of the cell.
    /// </summary>
    public double northing;

    /// <summary>
    /// The Easting coordinate value of the cell.
    /// </summary>
    public double easting;

    #region Equality test
    public override bool Equals(CellDatumResult other)
    {
      if (other == null)
        return false;

      var objToCompare = (CompactionCellDatumResult) other;

      return base.Equals(other) &&
             Math.Round(northing, 3) == Math.Round(objToCompare.northing, 3) &&
             Math.Round(easting, 3) == Math.Round(objToCompare.easting, 3);
    }

    public static bool operator ==(CompactionCellDatumResult a, CompactionCellDatumResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }
    public static bool operator !=(CompactionCellDatumResult a, CompactionCellDatumResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionCellDatumResult && this == (CompactionCellDatumResult)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }
}

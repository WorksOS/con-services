using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.Reports.StationOffset
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class StationOffsetReportDataRows : List<StationOffsetReportDataRow>, IEquatable<StationOffsetReportDataRows>
  {
    public StationOffsetReportDataRows()
    {
    }

    public void Write(BinaryWriter writer)
    {
      foreach (var griddedDataRow in this)
      {
        griddedDataRow.Write(writer);
      }
    }

    public void Read(BinaryReader reader, int numberOfRows)
    {
      for (int i = 0; i < numberOfRows; i++)
      {
        var grdr = new StationOffsetReportDataRow();
        grdr.Read(reader);
        Add(grdr);
      }
    }

    public bool Equals(StationOffsetReportDataRows other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      if (!base.Count.Equals(other.Count)) return false;
      for (int i = 0; i < base.Count; i++)
      {
        if (!base[i].Equals(other[i])) return false;
      }

      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StationOffsetReportDataRows) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        return hashCode;
      }
    }
  }
}

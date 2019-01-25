using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class StationOffsetReportDataRows_ApplicationService : List<StationOffsetReportDataRow_ApplicationService>, IEquatable<StationOffsetReportDataRows_ApplicationService>
  {
    public StationOffsetReportDataRows_ApplicationService()
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
        var temp = new StationOffsetReportDataRow_ApplicationService();
        temp.Read(reader);
        Add(temp);
      }
    }

    public bool Equals(StationOffsetReportDataRows_ApplicationService other)
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
      return Equals((StationOffsetReportDataRows_ApplicationService) obj);
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

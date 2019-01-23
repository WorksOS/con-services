using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  // todoJeannie move GriddedReportDataRow to a common path?
  public class OffsetStatistics : GriddedReportDataRow, IEquatable<OffsetStatistics>
  {
    public OffsetStatistics()
    {
    }

    public OffsetStatistics
    ( OffsetStatisticType type, double offset, double station, double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      // todo
      //switch (type)
      //{
      //  case OffsetStatisticType.Maximum:
      //  {
      //    base.SetValues(northing, easting, elevation, cutFill, cmv, mdp, passCount, temperature);
      //    offsetStatistics.SetValues(0, 0, station.ElevMax, station.CutFillMax, station.CMVMax, station.MDPMax, station.PassCountMax, station.TemperatureMax);
      //  }
      //    break;
      //  case OffsetStatisticType.Minimum:
      //  {
      //    offsetStatistics.SetValues(0, 0, station.ElevMin, station.CutFillMin, station.CMVMin, station.MDPMin, station.PassCountMin, station.TemperatureMin);
      //  }
      //    break;
      //  case OffsetStatisticType.Average:
      //  {
      //    offsetStatistics.SetValues(0, 0, station.ElevAvg, station.CutFillAvg, station.CMVAvg, station.MDPAvg, station.PassCountAvg, station.TemperatureAvg);
      //  }
      //  break;
      //}
      
    }

    public new void Write(BinaryWriter writer)
    {
      base.Write(writer);
    }

    public new void Read(BinaryReader reader)
    {
      base.Read(reader);
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public new void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public new void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
    }

    public bool Equals(OffsetStatistics other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((OffsetStatistics)obj);
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

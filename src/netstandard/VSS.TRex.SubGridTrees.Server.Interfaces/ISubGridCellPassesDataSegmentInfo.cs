using System;
using System.IO;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Interfaces
{
  public interface ISubGridCellPassesDataSegmentInfo
  {
    ISubGridCellPassesDataSegment Segment { get; set; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    double MinElevation { get; set; }
    double MaxElevation { get; set; }
    bool ExistsInPersistentStore { get; set; }
    DateTime MidTime { get; }

    /// <summary>
    /// IncludesTimeWithinBounds determines if ATime is strictly greater than
    /// the start time and strictly less than the end time of this segment.
    /// It is not intended to resolve boundary edge cases where ATime is exactly
    /// equal to the start or end time of the segment
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    bool IncludesTimeWithinBounds(DateTime time);

    string FileName(uint OriginX, uint OriginY);
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);

    ISubGridSpatialAffinityKey AffinityKey();
  }
}

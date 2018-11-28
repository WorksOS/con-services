using System;
using VSS.TRex.Cells;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// The SiteProofingRun describes a single proofing run made by a machine. 
  /// A proofing run is determined by the start and end time at which the run was made.
  /// </summary>
  public class SiteProofingRun : IEquatable<string>, ISiteProofingRun
  {
    public long MachineID { get; }

    public string Name { get; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public BoundingWorldExtent3D Extents { get; set; }

    /// <summary>
    /// The WorkingExtents is used as a working area for computing modified proofing run extents by operations such as data deletion. 
    /// It is not persisted in the proofing run description.
    /// </summary>
    public BoundingWorldExtent3D WorkingExtents { get; set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public SiteProofingRun()
    {
      Extents.Clear();
      WorkingExtents.Clear();
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machineID"></param>
    /// <param name="stratTime"></param>
    /// <param name="endTime"></param>
    /// <param name="extents"></param>
    public SiteProofingRun(string name, long machineID, DateTime stratTime, DateTime endTime, BoundingWorldExtent3D extents) : this()
    {
      Name = name;
      MachineID = machineID;
      StartTime = StartTime;
      EndTime = endTime;
      Extents = extents;
    }

    public bool MatchesCellPass(CellPass cellPass)
    {
      cellPass.MachineIDAndTime(out var machineID, out var time);

      return MachineID == machineID && DateTime.Compare(StartTime, time) <= 0 && DateTime.Compare(EndTime, time) >= 0;
    }

    public bool Equals(string other) => (other != null) && Name.Equals(other);
  }
}

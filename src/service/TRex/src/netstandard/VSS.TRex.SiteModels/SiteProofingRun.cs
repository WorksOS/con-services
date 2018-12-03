using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common.Utilities.Interfaces;
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
    public long MachineID { get; set; }

    public string Name { get; set; }

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

    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != UtilitiesConsts.ReaderWriterVersion)
        throw new Exception($"Invalid version number ({version}) reading proofing run data, expected version (1)");

      Name = reader.ReadString();
      MachineID = reader.ReadInt64();
      StartTime = DateTime.FromBinary(reader.ReadInt64());
      EndTime = DateTime.FromBinary(reader.ReadInt64());

      if (reader.ReadBoolean())
      {
        Extents = new BoundingWorldExtent3D();
        Extents.Read(reader);
      }
    }

    /// <summary>
    /// Serialises proofing run using the given writer
    /// </summary>
    /// <param name="writer"></param>

    public void Write(BinaryWriter writer)
    {
      writer.Write(UtilitiesConsts.ReaderWriterVersion);

      writer.Write(Name);
      writer.Write(MachineID);
      writer.Write(StartTime.ToBinary());
      writer.Write(EndTime.ToBinary());

      writer.Write(Extents != null);
      Extents?.Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);
  }
}

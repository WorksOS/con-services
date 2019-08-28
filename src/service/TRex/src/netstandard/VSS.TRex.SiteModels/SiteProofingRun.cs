using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Compression;
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
    private const byte VERSION_NUMBER = 1;

    public short MachineID { get; set; } = CellPassConsts.NullInternalSiteModelMachineIndex;

    public string Name { get; set; } = string.Empty;

    private DateTime _startTime = Consts.MIN_DATETIME_AS_UTC;

    public DateTime StartTime { get => _startTime;
      set
      {
        if (value.Kind != DateTimeKind.Utc)
          throw new ArgumentException("Proofing run start time must be a UTC date time", nameof(StartTime));
        _startTime = value;
      }
    }

    private DateTime _endTime = Consts.MIN_DATETIME_AS_UTC;

    public DateTime EndTime
    {
      get => _endTime;
      set
      {
        if (value.Kind != DateTimeKind.Utc)
          throw new ArgumentException("Proofing run end time must be a UTC date time", nameof(EndTime));
        _endTime = value;
      }
    }

    public BoundingWorldExtent3D Extents { get; set; } = new BoundingWorldExtent3D();

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public SiteProofingRun()
    {
      Extents.Clear();
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machineID"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="extents"></param>
    public SiteProofingRun(string name, short machineID, DateTime startTime, DateTime endTime, BoundingWorldExtent3D extents) : this()
    {
      Name = name;
      MachineID = machineID;
      StartTime = startTime;
      EndTime = endTime;
      Extents = extents;
    }

    public bool MatchesCellPass(CellPass cellPass)
    {
      cellPass.MachineIDAndTime(out var machineID, out var time);

      return MachineID == machineID && DateTime.Compare(StartTime, time) <= 0 && DateTime.Compare(EndTime, time) >= 0;
    }

    public bool Equals(string other) => other != null && Name.Equals(other);

    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Name = reader.ReadString();
      MachineID = reader.ReadInt16();
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
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(Name);
      writer.Write(MachineID);
      writer.Write(StartTime.ToBinary());
      writer.Write(EndTime.ToBinary());

      writer.Write(Extents != null);
      Extents?.Write(writer);
    }
  }
}

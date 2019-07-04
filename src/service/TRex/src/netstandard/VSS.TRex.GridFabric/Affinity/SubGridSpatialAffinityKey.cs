using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// The key type used to drive spatial affinity key mapping for elements stored in the Ignite cache. This controls
  /// which nodes in the PSNode layer the data for this key should reside. 
  /// </summary>
  public struct SubGridSpatialAffinityKey : ISubGridSpatialAffinityKey, IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    private long version;

    /// <summary>
    /// The version number of this spatial element when it is stored in the persistent layer, defined
    /// as the number of ticks in DateTime.UtcNow at the time it is written.
    /// </summary>
    public long Version { get => version; set => version = value; }

    public const long DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS = 1;

    private Guid projectUID;

    /// <summary>
    /// The GUID for the project the sub grid data belongs to.
    /// </summary>
    public Guid ProjectUID { get => projectUID; set => projectUID = value; }

    private int subGridX;

    /// <summary>
    /// The X ordinate cell address of the origin cell for the sub grid
    /// </summary>
    public int SubGridX { get => subGridX; set => subGridX = value; }

    private int subGridY;

    /// <summary>
    /// The Y ordinate cell address of the origin cell for the sub grid
    /// </summary>
    public int SubGridY { get => subGridY; set => subGridY = value; }

    private long segmentStartDateTicks;
    public long SegmentStartDateTicks { get => segmentStartDateTicks; set => segmentStartDateTicks= value; } // in ticks

    private long segmentEndDateTicks;
    public long SegmentEndDateTicks { get => segmentEndDateTicks; set => segmentEndDateTicks = value; } // in ticks

    /// <summary>
    /// A constructor for the sub grid spatial affinity key that accepts the project and sub grid origin location
    /// and returns an instance of the spatial affinity key
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    /// <param name="segmentStartDateTicks"></param>
    /// <param name="segmentEndDateTicks"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, int subGridX, int subGridY, long segmentStartDateTicks, long segmentEndDateTicks)
    {
      this.version = version;
      this.projectUID = projectID;
      this.subGridX = subGridX;
      this.subGridY = subGridY;
      this.segmentStartDateTicks = segmentStartDateTicks;
      this.segmentEndDateTicks = segmentEndDateTicks;
    }

    /// <summary>
    /// A constructor for the sub grid spatial affinity key that accepts the project and a cell address structure for
    /// the sub grid origin location and returns an instance of the spatial affinity key
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="address"></param>
    /// <param name="segmentStartDateTicks"></param>
    /// <param name="segmentEndDateTicks"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, SubGridCellAddress address, long segmentStartDateTicks, long segmentEndDateTicks)
    {
      this.version = version;
      this.projectUID = projectID;
      this.subGridX = address.X;
      this.subGridY = address.Y;
      this.segmentStartDateTicks = segmentStartDateTicks;
      this.segmentEndDateTicks = segmentEndDateTicks;
    }

    /// <summary>
    /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
    /// as the sub grid directory element
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, int subGridX, int subGridY) : this(version, projectID, subGridX, subGridY, -1, -1)
    {
    }

    /// <summary>
    /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
    /// as the sub grid directory element
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="address"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, SubGridCellAddress address) : this(version, projectID, address.X, address.Y, -1, -1)
    {
    }

    /// <summary>
    /// Converts the spatial segment affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return SegmentStartDateTicks == -1
        ? $"{projectUID}-{version}-{subGridX}-{subGridY}"
        : $"{projectUID}-{version}-{subGridX}-{subGridY}-{segmentStartDateTicks}-{segmentEndDateTicks}";
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(projectUID);
      writer.WriteInt(subGridX);
      writer.WriteInt(subGridY);
      writer.WriteLong(segmentStartDateTicks);
      writer.WriteLong(segmentEndDateTicks);
      writer.WriteLong(version);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      projectUID = reader.ReadGuid() ?? Guid.Empty;
      subGridX = reader.ReadInt();
      subGridY = reader.ReadInt();
      segmentStartDateTicks = reader.ReadLong();
      segmentEndDateTicks = reader.ReadLong();
      version = reader.ReadLong();
    }
  }
}

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

    private long _version;

    /// <summary>
    /// The version number of this spatial element when it is stored in the persistent layer, defined
    /// as the number of ticks in DateTime.UtcNow at the time it is written.
    /// </summary>
    public long Version { get => _version; set => _version = value; }

    public const long DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS = 1;

    private Guid _projectUid;

    /// <summary>
    /// The GUID for the project the sub grid data belongs to.
    /// </summary>
    public Guid ProjectUID { get => _projectUid; set => _projectUid = value; }

    private int _subGridX;

    /// <summary>
    /// The X ordinate cell address of the origin cell for the sub grid
    /// </summary>
    public int SubGridX { get => _subGridX; set => _subGridX = value; }

    private int _subGridY;

    /// <summary>
    /// The Y ordinate cell address of the origin cell for the sub grid
    /// </summary>
    public int SubGridY { get => _subGridY; set => _subGridY = value; }

    private long _segmentStartDateTicks;
    public long SegmentStartDateTicks { get => _segmentStartDateTicks; set => _segmentStartDateTicks= value; } // in ticks

    private long _segmentEndDateTicks;
    public long SegmentEndDateTicks { get => _segmentEndDateTicks; set => _segmentEndDateTicks = value; } // in ticks

    /// <summary>
    /// A constructor for the sub grid spatial affinity key that accepts the project and sub grid origin location
    /// and returns an instance of the spatial affinity key
    /// </summary>
    public SubGridSpatialAffinityKey(long version, Guid projectId, int subGridX, int subGridY, long segmentStartDateTicks, long segmentEndDateTicks)
    {
      _version = version;
      _projectUid = projectId;
      _subGridX = subGridX;
      _subGridY = subGridY;
      _segmentStartDateTicks = segmentStartDateTicks;
      _segmentEndDateTicks = segmentEndDateTicks;
    }

    /// <summary>
    /// A constructor for the sub grid spatial affinity key that accepts the project and a cell address structure for
    /// the sub grid origin location and returns an instance of the spatial affinity key
    /// </summary>
    public SubGridSpatialAffinityKey(long version, Guid projectId, SubGridCellAddress address, long segmentStartDateTicks, long segmentEndDateTicks)
    {
      _version = version;
      _projectUid = projectId;
      _subGridX = address.X;
      _subGridY = address.Y;
      _segmentStartDateTicks = segmentStartDateTicks;
      _segmentEndDateTicks = segmentEndDateTicks;
    }

    /// <summary>
    /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
    /// as the sub grid directory element
    /// </summary>
    public SubGridSpatialAffinityKey(long version, Guid projectId, int subGridX, int subGridY) : this(version, projectId, subGridX, subGridY, -1, -1)
    {
    }

    /// <summary>
    /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
    /// as the sub grid directory element
    /// </summary>
    public SubGridSpatialAffinityKey(long version, Guid projectId, SubGridCellAddress address) : this(version, projectId, address.X, address.Y, -1, -1)
    {
    }

    /// <summary>
    /// Converts the spatial segment affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    public override string ToString()
    {
      return SegmentStartDateTicks == -1
        ? $"{_projectUid}-{_version}-{_subGridX}-{_subGridY}"
        : $"{_projectUid}-{_version}-{_subGridX}-{_subGridY}-{_segmentStartDateTicks}-{_segmentEndDateTicks}";
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(_projectUid);
      writer.WriteInt(_subGridX);
      writer.WriteInt(_subGridY);
      writer.WriteLong(_segmentStartDateTicks);
      writer.WriteLong(_segmentEndDateTicks);
      writer.WriteLong(_version);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      _projectUid = reader.ReadGuid() ?? Guid.Empty;
      _subGridX = reader.ReadInt();
      _subGridY = reader.ReadInt();
      _segmentStartDateTicks = reader.ReadLong();
      _segmentEndDateTicks = reader.ReadLong();
      _version = reader.ReadLong();
    }
  }
}

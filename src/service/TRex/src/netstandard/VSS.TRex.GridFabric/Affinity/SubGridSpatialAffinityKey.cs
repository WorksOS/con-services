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

    /// <summary>
    /// The version number of this spatial element when it is stored in the persistent layer, defined
    /// as the number of ticks in DateTime.UtcNow at the time it is written.
    /// </summary>
    public long Version { get; set; }

    public const long DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER = 1; // tick

    /// <summary>
    /// The GUID for the project the sub grid data belongs to.
    /// </summary>
    public Guid ProjectUID { get; set; }

    /// <summary>
    /// The X ordinate cell address of the origin cell for the sub grid
    /// </summary>
    public uint SubGridX { get; set; }

    /// <summary>
    /// The Y ordinate cell address of the origin cell for the sub grid
    /// </summary>
    public uint SubGridY { get; set; }

    /// <summary>
    /// The segment identifier for the sub grid data. If the segment identifier is empty then the element represents
    /// the sub grid directory (or SGL file). Otherwise, the segment identifier is a string representation of the start
    /// and end times of the segment and the time duration the segment contains data for.
    /// </summary>
    public string SegmentIdentifier { get; set; }

    /// <summary>
    /// A constructor for the sub grid spatial affinity key that accepts the project and sub grid origin location
    /// and returns an instance of the spatial affinity key
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    /// <param name="segmentIdentifier"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, uint subGridX, uint subGridY, string segmentIdentifier)
    {
      Version = version;
      ProjectUID = projectID;
      SubGridX = subGridX;
      SubGridY = subGridY;
      SegmentIdentifier = segmentIdentifier;
    }

    /// <summary>
    /// A constructor for the sub grid spatial affinity key that accepts the project and a cell address structure for
    /// the sub grid origin location and returns an instance of the spatial affinity key
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="address"></param>
    /// <param name="segmentIdentifier"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, SubGridCellAddress address, string segmentIdentifier)
    {
      Version = version;
      ProjectUID = projectID;
      SubGridX = address.X;
      SubGridY = address.Y;
      SegmentIdentifier = segmentIdentifier;
    }

    /// <summary>
    /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
    /// as the sub grid directory element
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="subGridX"></param>
    /// <param name="subGridY"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, uint subGridX, uint subGridY) : this(version, projectID, subGridX, subGridY, "")
    {
    }

    /// <summary>
    /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
    /// as the sub grid directory element
    /// </summary>
    /// <param name="version"></param>
    /// <param name="projectID"></param>
    /// <param name="address"></param>
    public SubGridSpatialAffinityKey(long version, Guid projectID, SubGridCellAddress address) : this(version, projectID, address.X, address.Y, "")
    {
    }

    /// <summary>
    /// Converts the spatial segment affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return SegmentIdentifier == string.Empty
        ? $"{ProjectUID}-{SubGridX}-{SubGridY}"
        : $"{ProjectUID}-{SubGridX}-{SubGridY}-{SegmentIdentifier}";
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUID);
      writer.WriteInt((int) SubGridX);
      writer.WriteInt((int) SubGridY);
      writer.WriteString(SegmentIdentifier);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      SubGridX = (uint) reader.ReadInt();
      SubGridY = (uint) reader.ReadInt();
      SegmentIdentifier = reader.ReadString();
    }
  }
}

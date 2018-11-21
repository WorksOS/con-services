using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Exceptions;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// The key type used to drive non-spatial affinity key mapping for elements stored in the Ignite cache. This controls
  /// which nodes in the PSNode layer the data for this key should reside. 
  /// </summary>
  public struct NonSpatialAffinityKey : INonSpatialAffinityKey, IBinarizable, IFromToBinary, IEquatable<NonSpatialAffinityKey>
  {
    private const byte versionNumber = 1;

    /// <summary>
    /// The GUID for the project the subgrid data belongs to.
    /// </summary>
    public Guid ProjectUID { get; set; }

    /// <summary>
    /// Name of the object in the cache, encoded as a string
    /// </summary>
    public string KeyName { get; set; }

    /// <summary>
    /// A constructor for the affinity key that accepts the project and subgrid origin location
    /// and returns an instance of the spatial affinity key
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="keyName"></param>
    public NonSpatialAffinityKey(Guid projectID, string keyName)
    {
      ProjectUID = projectID;
      KeyName = keyName;
    }

    /// <summary>
    /// Converts the affinity key into a string representation suitable for use as a unique string
    /// identifying this data element in the cache.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{ProjectUID}-{KeyName}";

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(versionNumber);
      writer.WriteGuid(ProjectUID);
      writer.WriteString(KeyName);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      int version = reader.ReadByte();

      if (version != versionNumber)
        throw new TRexSerializationVersionException(versionNumber, version);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      KeyName = reader.ReadString();
    }

    public bool Equals(NonSpatialAffinityKey other)
    {
      return ProjectUID.Equals(other.ProjectUID) && string.Equals(KeyName, other.KeyName);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is NonSpatialAffinityKey other && Equals(other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (ProjectUID.GetHashCode() * 397) ^ (KeyName != null ? KeyName.GetHashCode() : 0);
      }
    }
  }
}

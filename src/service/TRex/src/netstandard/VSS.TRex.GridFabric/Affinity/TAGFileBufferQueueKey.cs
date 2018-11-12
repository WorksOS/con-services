using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Exceptions;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{

  /// <summary>
  /// The key used to identify TAG files in the TAG file buffer queue
  /// </summary>
  public struct TAGFileBufferQueueKey : ITAGFileBufferQueueKey, IBinarizable, IFromToBinary, IEquatable<TAGFileBufferQueueKey>
  {
    private const byte versionNumber = 1;

    /// <summary>
    /// The name of the TAG file being processed
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The project to process that TAG file into.
    /// This field also provides the affinity key mapping to the nodes in the mutable data grid
    /// </summary>
    public Guid ProjectUID { get; set; }

    public Guid AssetUID { get; set; }

    /// <summary>
    /// TAG File Buffer Queue key constructor taking project, asset and filename
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="projectID"></param>
    /// <param name="assetUid"></param>
    public TAGFileBufferQueueKey(string fileName, Guid projectID, Guid assetUid)
    {
      FileName = fileName;
      ProjectUID = projectID;
      AssetUID = assetUid;
    }

    /// <summary>
    /// Provides string representation of the state of the key
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}"; //$"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}";

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(versionNumber);
      writer.WriteGuid(ProjectUID);
      writer.WriteGuid(AssetUID);
      writer.WriteString(FileName);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      int version = reader.ReadByte();

      if (version != versionNumber)
        throw new TRexException($"Invalid version number ({version}) in {nameof(TAGFileBufferQueueKey)}, expected {versionNumber}");

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      AssetUID = reader.ReadGuid() ?? Guid.Empty;
      FileName = reader.ReadString();
    }

    public bool Equals(TAGFileBufferQueueKey other)
    {
      return string.Equals(FileName, other.FileName) && ProjectUID.Equals(other.ProjectUID) && AssetUID.Equals(other.AssetUID);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is TAGFileBufferQueueKey other && Equals(other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (FileName != null ? FileName.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ ProjectUID.GetHashCode();
        hashCode = (hashCode * 397) ^ AssetUID.GetHashCode();
        return hashCode;
      }
    }
  }
}

using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// The key used to identify machine spatial data change maps within projects
  /// </summary>
  public class SiteModelMachineAffinityKey : ISiteModelMachineAffinityKey, IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project to process that TAG file into.
    /// This field also provides the affinity key mapping to the nodes in the mutable data grid
    /// </summary>
    public Guid ProjectUID { get; set; }

    public Guid AssetUID { get; set; }

    public FileSystemStreamType StreamType { get; set; }

    public SiteModelMachineAffinityKey()
    {
    }

    /// <summary>
    /// TAG File Buffer Queue key constructor taking project, asset and filename
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="assetUid"></param>
    /// <param name="streamType"></param>
    public SiteModelMachineAffinityKey(Guid projectID, Guid assetUid, FileSystemStreamType streamType)
    {
      ProjectUID = projectID;
      AssetUID = assetUid;
      StreamType = streamType;
    }

    /// <summary>
    /// Provides string representation of the state of the key
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"Project: {ProjectUID}, Asset: {AssetUID}, StreamType:{StreamType}";

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUID);
      writer.WriteGuid(AssetUID);
      writer.WriteInt((int)StreamType);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      AssetUID = reader.ReadGuid() ?? Guid.Empty;
      StreamType = (FileSystemStreamType) reader.ReadInt();
    }
  }
}

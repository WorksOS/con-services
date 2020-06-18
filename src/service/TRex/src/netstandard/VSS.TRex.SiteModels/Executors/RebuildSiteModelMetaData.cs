using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelMetaData : IBinarizable, IFromToBinary
  {
    private static byte VERSION_NUMBER = 1;

    RebuildSiteModelPhase Phase { get; set; }

    /// <summary>
    /// The UTC date at which the last update to this metadata was made
    /// </summary>
    long LastUpdateUtcTicks { get; set; }

    /// <summary>
    /// Project being rebuilt
    /// </summary>
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The last known submitted TAG file
    /// </summary>
    public string LastSubmittedTagFile { get; set; }

    /// <summary>
    /// The last knwon processed TAG file
    /// </summary>
    public string LastProcessedTagFile { get; set; }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectUid = reader.ReadGuid() ?? Guid.Empty;
      LastUpdateUtcTicks = reader.ReadLong();
      Phase = (RebuildSiteModelPhase)reader.ReadInt();
      LastSubmittedTagFile = reader.ReadString();
      LastProcessedTagFile = reader.ReadString();
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUid);
      writer.WriteLong(LastUpdateUtcTicks);
      writer.WriteInt((int)Phase);
      writer.WriteString(LastSubmittedTagFile);
      writer.WriteString(LastProcessedTagFile);
    }

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialize this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialize this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}

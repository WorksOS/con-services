using System;
using Apache.Ignite.Core.Binary;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Requests;

namespace VSS.TRex.SiteModels.Executors
{
  public class RebuildSiteModelMetaData : IRebuildSiteModelMetaData, IBinarizable, IFromToBinary
  {
    private static byte VERSION_NUMBER = 1;

    /// <summary>
    /// The current phase of project rebuilding this project is in
    /// </summary>
    public RebuildSiteModelPhase Phase { get; set; }

    /// <summary>
    /// The UTC date at which the last update to this metadata was made
    /// </summary>
    public long LastUpdateUtcTicks { get; set; }

    /// <summary>
    /// Project being rebuilt
    /// </summary>
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// Defines how selective the site model delete operation should be.
    /// Selectivity allows certain portions of a site model to be deleted to help with operations
    /// like rebuilding projects on demand.
    /// </summary>
    public DeleteSiteModelSelectivity DeletionSelectivity { get; set; }

    /// <summary>
    /// The result of the deletion stage of the project rebuidl
    /// </summary>
    public DeleteSiteModelResult DeletionResult { get; set; }

    /// <summary>
    /// The result of this rebuild request. As this process may be long, this response will chiefly indicate the
    /// success or failure of starting the overall process of rebuilding a project.
    /// </summary>
    public RebuildSiteModelResult RebuildResult { get; set; }

    /// <summary>
    /// Denotes the type of S3 transfer proxy the site model rebuilder should use to scan and source TAG files for reprocessing
    /// This allows different sources such as the primary TAG file archive, or a prepared project migration bucket to be referenced
    /// </summary>
    public TransferProxyType OriginS3TransferProxy { get; set; }

    /// <summary>
    /// The number of tag files extracted from the S3 repository ready to submit for processing
    /// </summary>
    public int NumberOfTAGFilesFromS3 { get; set; }

    /// <summary>
    /// The last known submitted TAG file
    /// </summary>
    public string LastSubmittedTagFile { get; set; }

    /// <summary>
    /// The last known processed TAG file
    /// </summary>
    public string LastProcessedTagFile { get; set; }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectUid = reader.ReadGuid() ?? Guid.Empty;
      DeletionSelectivity = (DeleteSiteModelSelectivity)reader.ReadInt();

      OriginS3TransferProxy = (TransferProxyType)reader.ReadInt();
      NumberOfTAGFilesFromS3 = reader.ReadInt();

      LastUpdateUtcTicks = reader.ReadLong();
      Phase = (RebuildSiteModelPhase)reader.ReadInt();
      LastSubmittedTagFile = reader.ReadString();
      LastProcessedTagFile = reader.ReadString();

      DeletionResult = (DeleteSiteModelResult)reader.ReadInt();
      RebuildResult = (RebuildSiteModelResult)reader.ReadInt();
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUid);
      writer.WriteInt((int)DeletionSelectivity);

      writer.WriteInt((int)OriginS3TransferProxy);
      writer.WriteInt(NumberOfTAGFilesFromS3);

      writer.WriteLong(LastUpdateUtcTicks);
      writer.WriteInt((int)Phase);
      writer.WriteString(LastSubmittedTagFile);
      writer.WriteString(LastProcessedTagFile);

      writer.WriteInt((int)DeletionResult);
      writer.WriteInt((int)RebuildResult);
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

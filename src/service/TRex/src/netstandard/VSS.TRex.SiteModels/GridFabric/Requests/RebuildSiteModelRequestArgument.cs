using System;
using Apache.Ignite.Core.Binary;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SiteModels.Interfaces.Requests;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class RebuildSiteModelRequestArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project the request is relevant to
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// Defines how selective the site model delete operation should be.
    /// Selectivity allows certain portions of a site model to be deleted to help with operations
    /// like rebuilding projects on demand.
    /// </summary>
    public DeleteSiteModelSelectivity DeletionSelectivity { get; set; }

    /// <summary>
    /// Denotes the type of S3 transfer proxy the site model rebuilder should use to scan and source TAG files for reprocessing
    /// This allows different sources such as the primary TAG file archive, or a prepared project migration bucket to be referenced
    /// </summary>
    public TransferProxyType OriginS3TransferProxy { get; set; }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteInt((int)DeletionSelectivity);
      writer.WriteByte((byte)OriginS3TransferProxy);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      DeletionSelectivity = (DeleteSiteModelSelectivity)reader.ReadInt();
      OriginS3TransferProxy = (TransferProxyType)reader.ReadByte();
    }
  }
}

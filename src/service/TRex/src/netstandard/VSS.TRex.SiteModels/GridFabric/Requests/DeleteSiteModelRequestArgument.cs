using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class DeleteSiteModelRequestArgument : BaseRequestArgument
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
    public DeleteSiteModelSelectivity Selectivity { get; set; }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteInt((int)Selectivity);
  }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      Selectivity = (DeleteSiteModelSelectivity) reader.ReadInt();
    }
  }
}

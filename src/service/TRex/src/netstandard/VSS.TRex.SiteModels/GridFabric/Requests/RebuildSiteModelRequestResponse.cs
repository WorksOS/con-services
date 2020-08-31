using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  /// <summary>
  /// The response returned from the Grid request executor that contains the response code and the set of
  /// sub grids extracted for the grid report in question
  /// </summary>
  public class RebuildSiteModelRequestResponse : BaseRequestResponse
  {
    private static byte VERSION_NUMBER = 1;

    /// <summary>
    /// The project being rebuilt this response refers to
    /// </summary>
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The result of this rebuild request. As this process may be long, this response will chiefly indicate the
    /// success or failure of starting the overall process of rebuilding a project.
    /// </summary>
    public RebuildSiteModelResult RebuildResult;

    public RebuildSiteModelRequestResponse() { }

    public RebuildSiteModelRequestResponse(Guid projectUid)
    {
      ProjectUid = projectUid;
      RebuildResult = RebuildSiteModelResult.Pending;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectUid);
      writer.WriteByte((byte)RebuildResult);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectUid = reader.ReadGuid() ?? Guid.Empty;
        RebuildResult = (RebuildSiteModelResult) reader.ReadByte();
      }
    }
  }
}

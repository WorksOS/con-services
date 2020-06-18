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
    /// The result of any require project deletion activity before the main activity of rebuilding the project is started
    /// </summary>
    public DeleteSiteModelResult DeletionResult;

    /// <summary>
    /// The number of elements removed in the project deletion stage
    /// </summary>
    public long NumRemovedElements;

    /// <summary>
    /// The result of this rebuild request. As this process may be long, this response will chiefly indicate the 
    /// success or failure of starting the overall process of rebuilding a project.
    /// </summary>
    public RebuildSiteModelResult RebuildResult;


    public RebuildSiteModelRequestResponse()
    {
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)DeletionResult);
      writer.WriteLong(NumRemovedElements);

      writer.WriteInt((int)RebuildResult);

    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      DeletionResult = (DeleteSiteModelResult)reader.ReadInt();
      NumRemovedElements = reader.ReadLong();

      RebuildResult = (RebuildSiteModelResult)reader.ReadInt();
    }
  }
}

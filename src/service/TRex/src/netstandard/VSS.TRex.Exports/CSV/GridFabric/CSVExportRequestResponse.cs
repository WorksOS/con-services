using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// The response returned from the CSVExport request executor
  ///   that contains the response code and the set of formatted rows
  ///   extracted from the sub grids for the export question.
  /// </summary>
  public class CSVExportRequestResponse : SubGridsPipelinedResponseBase
  {
    public string fileName = string.Empty;

    public CSVExportRequestResponse()
    { }
    
    
     /// <summary>
     /// Serializes content to the writer
     /// </summary>
     /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteString(fileName);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      fileName = reader.ReadString();
    }
  }
}

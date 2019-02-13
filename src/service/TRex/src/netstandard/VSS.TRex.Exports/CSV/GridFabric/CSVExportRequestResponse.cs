using System.Collections.Generic;
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
    // todoJeannie base.ResultStatus
    //  0="Problem occured processing export."
    //  1="No Data"
    //  2="Timed out"
    //  3="Unknown Error"
    //  4="Cancelled"
    //  5="Maximum records reached"

    // todoJeannie byte[]? or just pass a HUGE string of ALL rows?
    // todoJeannie sort assuming northing and easting are first or do specific? 
    public List<string> dataRows;

    public CSVExportRequestResponse()
    {
      Clear();
    }

    private void Clear()
    {
      dataRows = new List<string>();
    }
    
    /// <summary>
     /// Serializes content to the writer
     /// </summary>
     /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteInt(dataRows.Count);
      foreach (var r in dataRows)
      {
        writer.WriteString(r);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      var count = reader.ReadInt();
      dataRows = new List<string>(count);
      for (int i = 0; i < count; i++)
      {
        dataRows.Add(reader.ReadString());
      }
    }
  }
}

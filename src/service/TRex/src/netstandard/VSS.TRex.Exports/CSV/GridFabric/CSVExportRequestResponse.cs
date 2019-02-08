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

    private string columnHeaders;

    // todoJeannie byte[]?
    // todoJeannie sort assuming northing and easting are first or do specific? 
    private string[] dataRows;

    public CSVExportRequestResponse()
    {
      Clear();
    }

    public void Clear()
    {
      columnHeaders = null;
      dataRows = new string[0];
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      //writer.WriteInt((int)ReturnCode);
      //writer.WriteInt((int)ReportType);
      //writer.WriteInt(GriddedReportDataRowList.Count);
      //for (int i = 0; i < GriddedReportDataRowList.Count; i++)
      //{
      //  GriddedReportDataRowList[i].ToBinary(writer);
      //}
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      //ReturnCode = (ReportReturnCode)reader.ReadInt();
      //ReportType = (ReportType)reader.ReadInt();
      //var griddedRowsCount = reader.ReadInt();
      //GriddedReportDataRowList = new List<GriddedReportDataRow>();
      //for (int i = 0; i < griddedRowsCount; i++)
      //{
      //  var row = new GriddedReportDataRow();
      //  row.FromBinary(reader);
      //  GriddedReportDataRowList.Add(row);
      //}
    }

    private void ColumnHeaders()
    {
      // procedure TICPassCountExportCalculator.SetupColumnHeaders;
      // requires UserPrefs: Units and request.CorrdinateType; OutputType   ()
      // todoJeannie translations?

    }
  }
}

using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class CompactionReportGridResult : ContractExecutionResult
  {
    public string ReportData { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionReportGridResult()
    { }

    /// <summary>
    /// Create an instance of the CompactionReportGridResult class
    /// </summary>
    public static CompactionReportGridResult CreateExportDataResult(string data, short resultCode)
    {
      return new CompactionReportGridResult { ReportData = data };
    }
  }
}

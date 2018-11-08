using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class CompactionReportResult : ContractExecutionResult
  {
    public ICompactionReport ReportData { get; private set; }

    /// <summary>
    /// Create an instance of the CompactionReportResult class
    /// </summary>
    public static CompactionReportResult CreateExportDataResult(ICompactionReport data, short resultCode)
    {
      return new CompactionReportResult { ReportData = data };
    }
  }
}
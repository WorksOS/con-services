using VSS.Common.ResultsHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class CompactionReportGridResult : ContractExecutionResult
  {
    public ICompactionReport ReportData { get; private set; }

    /// <summary>
    /// Create an instance of the CompactionReportGridResult class
    /// </summary>
    public static CompactionReportGridResult CreateExportDataResult(ICompactionReport data, short resultCode)
    {
      return new CompactionReportGridResult { ReportData = data };
    }
  }
}
using CCSS.WorksOS.Reports.Common.Models;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class DataGrabberResponse
  {
    public JModel ReportsData { get; set; }
    public int DataGrabberStatus { get; set; }
    public string Message { get; set; }

    public MandatoryReportData ReportData { get; set; }

    public virtual bool IsSummaryReport => false;

    public virtual bool IsGridReport => false;

    public virtual bool IsStationOffsetReport => false;
  }
}

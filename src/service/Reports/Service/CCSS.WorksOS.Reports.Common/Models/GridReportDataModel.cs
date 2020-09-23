using System.Collections.Specialized;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class GridReportDataModel : MandatoryReportData
  {
    //Dictionary of URLs to request
    public NameValueCollection ReportUrlQueryCollection { get; set; }

    //Actual data for the report
    public GridReportRow[] Rows { get; set; }
  }
}

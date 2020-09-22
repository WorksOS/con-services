using System.Collections.Generic;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public class Metadata
  {
    public string ReportTitle { get; set; }
    public string ReportGeneratedTime { get; set; }
    public string ReportWorkFlow { get; set; }
    public List<string> DataStrings = null;
  }
}

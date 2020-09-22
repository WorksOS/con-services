using System.Collections.Specialized;

namespace CCSS.WorksOS.Reports.Common.Models
{
  /// <summary>
  /// The report model for Station Offset report.
  /// </summary>
  public class StationOffsetReportDataModel : MandatoryReportData
  {
    // Gets or sets the URLs to request.
    public NameValueCollection ReportUrlQueryCollection { get; set; }

    /// <summary>
    /// Gets or sets the report data.
    /// </summary>
    public StationOffsetReportRow[] Rows { get; set; }
  }
}

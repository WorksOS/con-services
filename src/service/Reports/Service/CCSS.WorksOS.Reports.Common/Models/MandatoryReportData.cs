using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class MandatoryReportData
  {
    /// <summary>
    /// Gets or sets the Project name of the report.
    /// </summary>
    public ProjectV6DescriptorsSingleResult ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the Project extents used to set report dates whe the filter date range is project extents.
    /// </summary>
    public ProjectStatisticsResult ProjectExtents { get; set; }

    /// <summary>
    /// Gets or sets Filter descriptors used in the report.
    /// </summary>
    public FilterListData Filters { get; set; }

    /// <summary>
    /// Gets or sets the report filter.
    /// </summary>
    public Filter ReportFilter { get; set; }

    /// <summary>
    /// Gets or sets the imported files for Filters and Cut\Fill.
    /// </summary>
    public ImportedFileDescriptorListResult ImportedFiles { get; set; }
  }
}

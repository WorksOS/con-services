using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class PassCountSummary : SummaryDataBase
  {
    public PassCountSummaryData PassCountSummaryData { get; set; }
    [JsonIgnore]
    public bool IsEmpty => PassCountSummaryData == null;
  }
}

using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class CMVSummary : SummaryDataBase
  {
    public CmvSummaryData CmvSummaryData { get; set; }

    [JsonIgnore]
    public bool IsEmpty => CmvSummaryData == null;
  }
}

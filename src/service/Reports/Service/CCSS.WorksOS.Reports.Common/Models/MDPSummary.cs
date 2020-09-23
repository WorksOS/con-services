using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class MDPSummary : SummaryDataBase
  {
    public MdpSummaryData MdpSummaryData { get; set; }

    [JsonIgnore]
    public bool IsEmpty => MdpSummaryData == null;
  }
}

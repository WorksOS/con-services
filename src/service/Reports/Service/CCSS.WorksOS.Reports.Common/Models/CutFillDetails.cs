using System.Linq;
using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class CutFillDetails : SummaryDataBase
  {
    /// <summary>
    /// An array of percentages relating to the cut-fill tolerances.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Percents.All(p => p == 0);
  }
}

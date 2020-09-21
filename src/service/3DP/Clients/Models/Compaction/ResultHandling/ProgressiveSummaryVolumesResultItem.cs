using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  public class ProgressiveSummaryVolumesResultItem : ContractExecutionResult
  {
    /// <summary>
    /// Start date in UTC of the interval this volume is computed for
    /// </summary>
    [JsonProperty("date")]
    public DateTime Date { get; private set; }

    /// <summary>
    /// The summary volume calculated between data and date + interval
    /// </summary>
    [JsonProperty("volume")]
    public SummaryVolumesResult Volume { get; private set; }

    public static ProgressiveSummaryVolumesResultItem Create(DateTime date, SummaryVolumesResult volume)
    {
      return new ProgressiveSummaryVolumesResultItem {Date = date, Volume = volume};
    }
  }
}

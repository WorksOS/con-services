using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents a single volume element in the progressive series
  /// </summary>
  public class ProgressiveCompactionVolumesSummaryDataItem
  {
    [JsonProperty(PropertyName = "date")] 
    public DateTime Date { get; private set; }

    [JsonProperty(PropertyName = "volume")]
    public VolumesSummaryData Volume { get; private set; }

    private ProgressiveCompactionVolumesSummaryDataItem()
    {}

    public static ProgressiveCompactionVolumesSummaryDataItem Create(DateTime date, VolumesSummaryData volume)
    {
      return new ProgressiveCompactionVolumesSummaryDataItem {Date = date, Volume = volume};
    }
  }
}

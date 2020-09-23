using System.Linq;
using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class TemperatureDetails : SummaryDataBase
  {
    public TemperatureDetailsData TemperatureDetailsData { get; set; }

    //Temperature details, unlike all the others, doesn't have a nested data model from 3dpm
    [JsonIgnore]
    public bool IsEmpty => TemperatureDetailsData?.IsEmpty ?? true;
  }

  public class TemperatureDetailsData
  {
    /// <summary>
    /// An array of percentages relating to the temperature targets.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }

    /// <summary>
    /// Temperature machine target range and whether it is constant or varies.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureTarget")]
    public TemperatureTargetData TemperatureTarget { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Percents == null || Percents.Length == 0 || Percents.All(p => p == 0);
  }
}


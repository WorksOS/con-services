using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class ElevationPalette : SummaryDataBase
  {
    /// <summary>
    /// The palette for displaying elevation values.
    /// </summary>
    [JsonProperty(PropertyName = "palette", Required = Required.Default)]
    public DetailPalette Palette { get; private set; }
  }
}

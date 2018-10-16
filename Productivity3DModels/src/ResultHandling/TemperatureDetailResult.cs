using System;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;


namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Result class for temperature detail report
  /// </summary>
  public class TemperatureDetailResult : TemperatureBaseResult
  {

    /// <summary>
    /// Collection of temperature percentages where each element represents the percentage of the matching index temperature number provided in the 
    /// temperatureCounts member of the temperature count request representation.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }


    /// <summary>
    /// Gets whether the temperature percent result object contains data.
    /// </summary>
    /// <returns></returns>
    public bool HasData() => (Percents?.Any(d => Math.Abs(d) > 0.001) ?? false);

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private TemperatureDetailResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="targetData"></param>
    /// <param name="percents"></param>
    public TemperatureDetailResult(TemperatureTargetData targetData, double[] percents)
    {
      TargetData = targetData;
      Percents = percents;
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the array of temperature percentages</returns>
    public override string ToString()
    {
      return
        $"percents:{string.Join("%, ", this.Percents) + "%"}";
    }

  }
}

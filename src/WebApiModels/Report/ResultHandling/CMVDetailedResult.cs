using System;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// The result representation of a detailed CMV request
  /// </summary>
  public class CMVDetailedResult : ContractExecutionResult
  {
    /// <summary>
    /// An array of percentages relating to the CMV values encountered in the processed cells.
    /// The percentages are for CMV values below the minimum, between the minimum and target, on target, between the target and the maximum and above the maximum CMV.
    /// </summary>
    [JsonProperty(PropertyName = "percents")]
    public double[] Percents { get; private set; }

    /// <summary>
    /// Gets whether the CMV Details result object contains data.
    /// </summary>
    /// <remarks>
    /// If the Percents array contains zero data then the result has no data.
    /// </remarks>
    /// <returns></returns>
    public bool HasData() => Percents?.Any(d => Math.Abs(d) > 0.001) ?? false;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CMVDetailedResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static CMVDetailedResult Create(double[] percents)
    {
      return new CMVDetailedResult
      {
        Percents = percents
      };
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A comma separated list of the percentages in the array.</returns>
    public override string ToString()
    {
      return string.Join("%, ", Percents) + "%";
    }
  }
}

using System;
using System.Linq;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a detailed CMV request
  /// </summary>
  public class CMVDetailedResult : CMVBaseResult
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
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="percents"></param>
    /// <param name="constantTargetCmv"></param>
    /// <param name="isTargetCmvConstant"></param>
    public CMVDetailedResult(
      double[] percents, 
      short constantTargetCmv = -1,
      bool isTargetCmvConstant = false
     )
    {
      Percents = percents;
      ConstantTargetCmv = constantTargetCmv;
      IsTargetCmvConstant = isTargetCmvConstant;
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A comma separated list of the percentages in the array.</returns>
    public override string ToString()
    {
      return $"Percents: {string.Join("%, ", Percents)}%, IsTargetCmvConstant: {IsTargetCmvConstant}, ConstantTargetCmv: {ConstantTargetCmv}";
    }
  }
}


using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// The result representation of a summary MDP request
  /// </summary>
  public class MDPSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The percentage of cells that are compacted within the target bounds
    /// </summary>
    public double compactedPercent { get; private set; }

    /// <summary>
    /// If the MDP value is constant, this is the constant value of all MDP targets in the processed data.
    /// </summary>
    public short constantTargetMDP { get; private set; }

    /// <summary>
    /// Are the MDP target values applying to all processed cells constant?
    /// </summary>
    public bool isTargetMDPConstant { get; private set; }

    /// <summary>
    /// The percentage of the cells that are over-compacted
    /// </summary>
    public double overCompactedPercent { get; private set; }

    /// <summary>
    /// The internal result code of the request. Documented elsewhere.
    /// </summary>
    public short returnCode { get; private set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    public double totalAreaCoveredSqMeters { get; private set; }

    /// <summary>
    /// The percentage of the cells that are under compacted
    /// </summary>
    public double underCompactedPercent { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MDPSummaryResult()
    { }

    /// <summary>
    /// Create instance of MDPSummaryResult
    /// </summary>
    public static MDPSummaryResult CreateMDPSummaryResult(
      double compactedPercent,
      short constantTargetMDP,
      bool isTargetMDPConstant,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      return new MDPSummaryResult
      {
        compactedPercent = compactedPercent,
        constantTargetMDP = constantTargetMDP,
        isTargetMDPConstant = isTargetMDPConstant,
        overCompactedPercent = overCompactedPercent,
        returnCode = returnCode,
        totalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
        underCompactedPercent = underCompactedPercent
      };
    }

  }
}

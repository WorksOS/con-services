
using ASNodeDecls;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// The result representation of a summary CMV request
  /// </summary>
  public class CMVSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// The percentage of cells that are compacted within the target bounds
    /// </summary>
    public double compactedPercent { get; private set; }

    /// <summary>
    /// If the CMV value is constant, this is the constant value of all CMV targets in the processed data.
    /// </summary>
    public short constantTargetCMV { get; private set; }

    /// <summary>
    /// Are the CMV target values applying to all processed cells constant?
    /// </summary>
    public bool isTargetCMVConstant { get; private set; }

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
    private CMVSummaryResult() 
    {}

    /// <summary>
    /// Create instance of CMVSummaryResult
    /// </summary>
    public static CMVSummaryResult CreateCMVSummaryResult(
      double compactedPercent,
      short constantTargetCMV,
      bool isTargetCMVConstant,
      double overCompactedPercent,
      short returnCode,
      double totalAreaCoveredSqMeters,
      double underCompactedPercent
      )
    {
      return new CMVSummaryResult
      {
        compactedPercent = compactedPercent,
        constantTargetCMV = constantTargetCMV,
        isTargetCMVConstant = isTargetCMVConstant,
        overCompactedPercent = overCompactedPercent,
        returnCode = returnCode,
        totalAreaCoveredSqMeters = totalAreaCoveredSqMeters,
        underCompactedPercent = underCompactedPercent
      };
    }

    /// <summary>
    /// Create example instance of CMVSummaryResult to display in Help documentation.
    /// </summary>
    public static CMVSummaryResult HelpSample
    {
      get
      {
        return new CMVSummaryResult()
        {
          compactedPercent = 50.0,
          constantTargetCMV = 95,
          isTargetCMVConstant = false,
          overCompactedPercent = 12.7,
          returnCode = (short)TASNodeErrorStatus.asneOK,
          totalAreaCoveredSqMeters = 18476.54,
          underCompactedPercent = 37.3
        };
      }
    }

  }
}
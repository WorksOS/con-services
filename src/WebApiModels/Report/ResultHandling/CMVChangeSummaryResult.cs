
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary CMV Change request
  /// </summary>
  public class CMVChangeSummaryResult : ContractExecutionResult
  {
    protected CMVChangeSummaryResult(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CMVChangeSummaryResult()
    {
    }

    /// <summary>
    /// Percent of the cells meeting values request conditions
    /// </summary>
    public double[] Values { get; private set; }

    /// <summary>
    /// Gets the coverage area where we have not null measured CCV
    /// </summary>
    public double CoverageArea { get; private set; }


    public static CMVChangeSummaryResult CreateSummarySpeedResult(double[] values, double coverageArea)
    {
      return new CMVChangeSummaryResult()
             {
                 Values = values,
                 CoverageArea = coverageArea
             };
    }

    /// <summary>
    /// Create example instance of SummaryVolumesResult to display in Help documentation.
    /// </summary>
    public static CMVChangeSummaryResult HelpSample
    {
      get
      {
        return new CMVChangeSummaryResult()
               {
                   Values = new double[] {23, 543, 1233, 633}
               };
      }

    }
  }
}
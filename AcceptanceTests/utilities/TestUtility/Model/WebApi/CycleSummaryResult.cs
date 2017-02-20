namespace TestUtility.Model.WebApi
{
  /// <summary>
  /// The result of a request to get cycle count summary data
  /// </summary>
  public class CycleSummaryResult : ContractExecutionResult
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public CycleSummaryResult()
      : this(ContractExecutionStatesEnum.ExecutedSuccessfully, DefaultMessage)
    {
    }

    /// <summary>
    /// Constructor for error code and message
    /// </summary>
    public CycleSummaryResult(ContractExecutionStatesEnum code, string message)
      : base(code, message)
    {
    }

    /// <summary>
    /// Total, target and average cycle counts for the assets for the day.
    /// </summary>
    public CycleSummaryData day { get; set; }

    /// <summary>
    /// Total, target and average cycle counts for the assets for the week.
    /// </summary>
    public CycleSummaryData week { get; set; }

    /// <summary>
    /// Total, target and average cycle counts for the assets for the month.
    /// </summary>
    public CycleSummaryData month { get; set; }
  }
}

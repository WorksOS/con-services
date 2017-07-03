using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Contracts
{
  /// <summary>
  ///   Defines standard return codes for a contract.
  /// </summary>
  public class ContractExecutionStatesEnum : GenericEnum<ContractExecutionStatesEnum, int>
  {
    /// <summary>
    /// The default execution result offset to create dynamically added custom errors
    /// </summary>
    private const int executionResultOffset = 100;

    /// <summary>
    /// The execution result offset to create a second set of dynamically added custom errors
    /// </summary>
    private const int executionResultOffset2 = 200;

    public int DefaultDynamicOffset => executionResultOffset;
    public int SecondDynamicOffset => executionResultOffset2;

    /// <summary>
    ///   Service request executed successfully
    /// </summary>
    public static readonly int ExecutedSuccessfully = 0;

    /// <summary>
    ///   Requested data was invalid or POSTed JSON was invalid
    /// </summary>
    public static readonly int IncorrectRequestedData = -1;

    /// <summary>
    ///   Supplied data didn't pass validation
    /// </summary>
    public static readonly int ValidationError = -2;

    /// <summary>
    ///   Internal processing error
    /// </summary>
    public static readonly int InternalProcessingError = -3;

    /// <summary>
    ///   Failed to get results
    /// </summary>
    public static readonly int FailedToGetResults = -4;

    /// <summary>
    ///   Failed to authorize for the project
    /// </summary>
    public static readonly int AuthError = -5;

    /// <summary>
    ///   Failed to authorize for the project
    /// </summary>
    public static readonly int PartialData = -6;

    /// <summary>
    /// Asset does not have a valid subscription for specified date
    /// </summary>
    public static readonly int NoSubscription = -7;

    /// <summary>
    /// Dynamically adds new error messages with specified offset.
    /// </summary>
    /// <param name="name">The name of error.</param>
    /// <param name="value">The value of code.</param>
    /// <param name="offset">The offset to use.</param>
    public void DynamicAddwithOffset(string name, int value, int offset = executionResultOffset)
    {
      DynamicAdd(name, value + offset);
    }

    /// <summary>
    /// Gets the error number with specified offset.
    /// </summary>
    /// <param name="errorNum">The error number.</param>
    /// <param name="offset">The offset.</param>
    /// <returns></returns>
    public int GetErrorNumberwithOffset(int errorNum, int offset = executionResultOffset)
    {
      return errorNum + offset;
    }

    /// <summary>
    /// Gets the first available name of an error code taking into account 
    /// </summary>
    /// <param name="value">The code value to get the name against.</param>
    /// <param name="offset">The offset.</param>
    /// <returns></returns>
    public string FirstNameWithOffset(int value, int offset = executionResultOffset)
    {
      return FirstNameWith(value + offset);
    }
  }
}
using System;

namespace VSS.MasterData.Models.ResultHandling.Abstractions
{
  public interface IErrorCodesProvider
  {
    /// <summary>
    /// Dynamically adds new error messages addwith offset.
    /// </summary>
    /// <param name="name">The name of error.</param>
    /// <param name="value">The value of code.</param>
    void DynamicAddwithOffset(string name, int value);

    /// <summary>
    /// Gets the error numberwith offset.
    /// </summary>
    /// <param name="errorNum">The error number.</param>
    /// <returns></returns>
    int GetErrorNumberwithOffset(int errorNum);

    /// <summary>
    /// Gets the frist available name of a error code taking into account 
    /// </summary>
    /// <param name="value">The code vale to get the name against.</param>
    /// <returns></returns>
    string FirstNameWithOffset(int value);

    /// <summary>
    /// Gets the dynamic count.
    /// </summary>
    /// <value>
    /// The dynamic count.
    /// </value>
    int DynamicCount { get; }

    /// <summary>
    /// Gets the names.
    /// </summary>
    /// <returns></returns>
    string[] GetNames();
    /// <summary>
    /// Indexes the of.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    int IndexOf(string name);
    /// <summary>
    /// Names at.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns></returns>
    string NameAt(int index);
    /// <summary>
    /// Gets the type of the underlying.
    /// </summary>
    /// <value>
    /// The type of the underlying.
    /// </value>
    Type UnderlyingType { get; }
    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>
    /// The count.
    /// </value>
    int Count { get; }
    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    /// <value>
    /// The index.
    /// </value>
    int Index { get; set; }
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; set; }
    /// <summary>
    /// Determines whether [is defined name] [the specified name].
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>
    ///   <c>true</c> if [is defined name] [the specified name]; otherwise, <c>false</c>.
    /// </returns>
    bool IsDefinedName(string name);
    /// <summary>
    /// Determines whether [is defined index] [the specified index].
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>
    ///   <c>true</c> if [is defined index] [the specified index]; otherwise, <c>false</c>.
    /// </returns>
    bool IsDefinedIndex(int index);
    /// <summary>
    /// Clears the dynamic.
    /// </summary>
    void ClearDynamic();
  }


  /// <summary>
  /// Defines standard return codes for a contract.
  /// </summary>
  public class ContractExecutionStatesEnum : GenericEnum<ContractExecutionStatesEnum, int>, IErrorCodesProvider
  {
    /// <summary>
    /// The execution result offset to create dynamically add custom errors
    /// </summary>
    protected virtual int executionResultOffset { get; } = 2000;

    /// <summary>
    /// Service request executed successfully
    /// </summary>
    public static readonly int ExecutedSuccessfully = ExecutedSuccessfullyConst;
    public const int ExecutedSuccessfullyConst = 0;

    /// <summary>
    /// Supplied data didn't pass validation
    /// </summary>
    public static readonly int ValidationError = ValidationErrorConst;
    public const int ValidationErrorConst = -1;

    /// <summary>
    /// Serializing request erors
    /// </summary>
    public static readonly int SerializationError = SerializationErrorConst;
    public const int SerializationErrorConst = -2;

    /// <summary>
    /// Internal processing error
    /// </summary>
    public static readonly int InternalProcessingError = InternalProcessingErrorConst;
    public const int InternalProcessingErrorConst = -3;

    /// <summary>
    /// Failed to get results
    /// </summary>
    public static readonly int FailedToGetResults = FailedToGetResultsConst;
    public const int FailedToGetResultsConst = -4;

    /// <summary>
    /// Failed to authorize for the project
    /// </summary>
    public static readonly int AuthError = AuthErrorConst;
    public const int AuthErrorConst = -5;

    /// <summary>
    /// Failed to authorize for the project
    /// </summary>
    public static readonly int PartialData = PartialDataConst;
    public const int PartialDataConst = -6;

    /// <summary>
    /// Asset does not have a valid subscription for specified date
    /// </summary>
    public static readonly int NoSubscription = NoSubscriptionConst;
    public const int NoSubscriptionConst = -7;

    /// <summary>
    /// Dynamically adds new error messages addwith offset.
    /// </summary>
    /// <param name="name">The name of error.</param>
    /// <param name="value">The value of code.</param>
    public void DynamicAddwithOffset(string name, int value)
    {
      DynamicAdd(name, value + executionResultOffset);
    }

    /// <summary>
    /// Gets the error numberwith offset.
    /// </summary>
    /// <param name="errorNum">The error number.</param>
    /// <returns></returns>
    public int GetErrorNumberwithOffset(int errorNum)
    {
      return errorNum + executionResultOffset;
    }

    /// <summary>
    /// Gets the first available name of a error code taking into account .
    /// </summary>
    /// <param name="value">The code value to get the name against.</param>
    public string FirstNameWithOffset(int value)
    {
      return FirstNameWith(value + executionResultOffset);
    }
  }
}

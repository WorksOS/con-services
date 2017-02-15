using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.ResultHandling
{
  /// <summary>
  /// The result representation of an application alarm request.
  /// </summary>
  public class AppAlarmResult : ContractExecutionResult //  IHelpSample
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private AppAlarmResult()
    //{ }

    /// <summary>
    /// Create instance of AppAlarmResult
    /// </summary>
    public static AppAlarmResult CreateAppAlarmResult(bool result)
    {
      return new AppAlarmResult
      {
        result = result
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static AppAlarmResult HelpSample
    {
      get { return CreateAppAlarmResult(true); }
    }
  }
}
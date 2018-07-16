namespace VSS.TRex.Analytics.Foundation.Interfaces
{
  /// <summary>
  /// Defines an interface for a generic extension to an analytics response class to provide a
  /// conversion from the internal respose state to the external result state
  /// </summary>
  /// <typeparam name="TResult"></typeparam>
  public interface IAnalyticsOperationResponseResultConversion<TResult>
  {
    /// <summary>
    /// Constructs the desired result from the response state
    /// </summary>
    /// <returns></returns>
    TResult ConstructResult();
  }
}

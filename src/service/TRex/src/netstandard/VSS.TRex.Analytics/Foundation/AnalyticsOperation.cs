using System.Threading.Tasks;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.Foundation
{
  public class AnalyticsOperation<TRequest_ApplicationService, TArgument, TResponse, TResult> : IAnalyticsOperation<TArgument, TResult>
      where TRequest_ApplicationService : IGenericASNodeRequest<TArgument, TResponse>, new()
      where TResponse : class, IAnalyticsOperationResponseResultConversion<TResult>, new()
      where TResult : AnalyticsResult, new()
  {
    /// <summary>
    /// Execute the analytics operation with the supplied argument synchronously.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public TResult Execute(TArgument arg)
    {
      var request = new TRequest_ApplicationService();

      return request.Execute(arg).ConstructResult();
    }

    /// <summary>
    /// Execute the analytics operation with the supplied argument asynchronously.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public async Task<TResult> ExecuteAsync(TArgument arg)
    {
      var request = new TRequest_ApplicationService();

      return (await request.ExecuteAsync(arg)).ConstructResult();
    }
  }
}

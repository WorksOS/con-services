using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.Foundation
{
    public class AnalyticsOperation<TRequest_ApplicationService, TArgument, TResponse, TResult> : IAnalyticsOperation<TArgument, TResult>
        where TRequest_ApplicationService : IGenericASNodeRequest<TArgument, TResponse>, new()
        where TResponse : class, IAnalyticsOperationResponseResultConversion<TResult>, new()
        where TResult : AnalyticsResult, new()
    {
        /// <summary>
        /// Execute the analytics operation with the supplied argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public TResult Execute(TArgument arg)
        {
            var request = new TRequest_ApplicationService();

            return request.Execute(arg).ConstructResult();
        }
    }
}

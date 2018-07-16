using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs
{
    /// <summary>
    /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
    /// perform subgrid processing.
    /// </summary>
    public class AnalyticsComputeFunc_ApplicationService<TArgument, TResponse, TRequest> : BaseComputeFunc, IComputeFunc<TArgument, TResponse>
        where TArgument : class
        where TResponse : class, IAggregateWith<TResponse>
        where TRequest : BaseRequest<TArgument, TResponse>, new()
    {
        [NonSerialized]
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public TResponse Invoke(TArgument arg)
        {
            Log.LogInformation("In AnalyticsComputeFunc_ApplicationService.Invoke()");

            try
            {
                TRequest request = new TRequest();

                Log.LogInformation("Executing AnalyticsComputeFunc_ApplicationService.Execute()");

                return request.Execute(arg);
            }
            finally
            {
                Log.LogInformation("Exiting AnalyticsComputeFunc_ApplicationService.Invoke()");
            }
        }
    }
}

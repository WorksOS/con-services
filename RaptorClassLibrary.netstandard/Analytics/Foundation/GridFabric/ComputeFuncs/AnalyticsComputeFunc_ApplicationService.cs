using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.GridFabric.ComputeFuncs
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TResponse Invoke(TArgument arg)
        {
            Log.Info("In AnalyticsComputeFunc_ApplicationService.Invoke()");

            try
            {
                TRequest request = new TRequest();

                Log.Info("Executing AnalyticsComputeFunc_ApplicationService.Execute()");

                return request.Execute(arg);
            }
            finally
            {
                Log.Info("Exiting AnalyticsComputeFunc_ApplicationService.Invoke()");
            }
        }
    }
}

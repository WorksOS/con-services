using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.Models;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.ComputeFuncs
{
    /// <summary>
    /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
    /// perform subgrid processing.
    /// </summary>
    public class AnalyticsComputeFunc_ApplicationService<TArgument, TResponse, TRequest> : BaseRaptorComputeFunc, IComputeFunc<TArgument, TResponse>
        where TArgument : class, new()
        where TResponse : class, new()
        where TRequest : class, IComputeFunc<TArgument, TResponse>, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TResponse Invoke(TArgument arg)
        {
            Log.Info("In AnalyticsComputeFunc_ApplicationService.Invoke()");

            try
            {
                TRequest request = new TRequest();

                Log.Info("Executing AnalyticsComputeFunc_ApplicationService.Execute()");

                return request.Invoke(arg);
            }
            finally
            {
                Log.Info("Exiting AnalyticsComputeFunc_ApplicationService.Invoke()");
            }
        }

    }
}

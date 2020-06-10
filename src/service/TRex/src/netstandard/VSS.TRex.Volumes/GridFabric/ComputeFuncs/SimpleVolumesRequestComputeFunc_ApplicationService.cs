using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Executors;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.ComputeFuncs
{
    /// <summary>
    /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
    /// perform sub grid processing.
    /// </summary>
    public class SimpleVolumesRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        private static readonly ILogger _log = Logging.Logger.CreateLogger<SimpleVolumesRequestComputeFunc_ApplicationService>();

        /// <summary>
        /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
        /// </summary>
        public SimpleVolumesRequestComputeFunc_ApplicationService()
        {
        }

        /// <summary>
        /// Invokes the simple volumes request with the given simple volumes request argument
        /// </summary>
        public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
        {
            _log.LogInformation($"In {nameof(Invoke)}");

            try
            {
                var executor = new SimpleVolumesExecutor();
                var response = executor.ExecuteAsync(arg).WaitAndUnwrapException();

                _log.LogInformation($"Simple volumes result is {response}");

                return response;
            }
            finally
            {
                _log.LogInformation($"Exiting {nameof(Invoke)}");
            }
        }
    }
}

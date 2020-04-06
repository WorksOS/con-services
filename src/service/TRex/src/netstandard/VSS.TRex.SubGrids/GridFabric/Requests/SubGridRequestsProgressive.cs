using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.GridFabric.Listeners;
using VSS.TRex.SubGrids.Responses;

namespace VSS.TRex.SubGrids.GridFabric.Requests
{
    /// <summary>
    /// Requests sub grids from the cache compute cluster allowing in-progress updates of results to be sent back to
    /// the calling context via a sub grid listener for processing.
    /// </summary>
    public class SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, IAggregateWith<TSubGridRequestsResponse>, new()
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

        private readonly IPipelineListenerMapper _listenerMapper = DIContext.Obtain<IPipelineListenerMapper>();

        /// <summary>
        /// The listener to which the processing engine may send in-progress updates during processing of the overall sub grids request
        /// </summary>
        private SubGridListener Listener { get; set; }

        /// <summary>
        /// Default no-arg constructor that delegates construction to the base class
        /// </summary>
        public SubGridRequestsProgressive()
        {
        }

        /// <summary>
        /// Creates the sub grid listener on the task defined in the argument to be sent to the cache cluster
        /// </summary>
        private void CreateSubGridListener()
        {
            // Create any required listener for periodic responses directly sent from the processing context to this context
            Listener = new SubGridListener(TRexTask);
            StartListening();
        }

        private void StartListening()
        {
            _listenerMapper.Add(TRexTask.RequestDescriptor, Listener);
        }

        private void StopListening()
        {
            _listenerMapper.Remove(TRexTask.RequestDescriptor, Listener);
            Listener = null;
        }

        private void PrepareForExecution()
        {
          CheckArguments();

          // Construct the argument to be supplied to the compute cluster
          PrepareArgument();

          Log.LogInformation($"Prepared argument has TRexNodeId = {arg.TRexNodeID}");
          Log.LogInformation($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} sub grids");
          Log.LogInformation($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()} sub grids");

          CreateSubGridListener();
        }

        /// <summary>
        /// Overrides the base Execute() semantics to add a listener available for in-progress updates of information
        /// from the processing engine.
        /// </summary>
        /// <returns></returns>
        public override TSubGridRequestsResponse Execute()
        {
            PrepareForExecution();

            Task<ICollection<TSubGridRequestsResponse>> taskResult = null;

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                // Construct the function to be used
                var func = new SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>();

                taskResult = Compute.BroadcastAsync(func, arg);
                taskResult.Wait(30000);
            }
            finally
            {
                sw.Stop();
                Log.LogInformation($"TaskResult {taskResult?.Status}: SubGridRequests.Execute() for DM:{TRexTask.PipeLine.DataModelID} from node {TRexTask.TRexNodeID} for data type {TRexTask.GridDataType} took {sw.ElapsedMilliseconds}ms");
            }

            // Send the appropriate response to the caller
            return taskResult.Result?.Count > 0 
              ? taskResult.Result.Aggregate((first, second) => (TSubGridRequestsResponse) first.AggregateWith(second)) 
              : null;
        }

        /// <summary>
        /// Overrides the base ExecuteAsync() semantics to add a listener available for in-progress updates of information
        /// from the processing engine.
        /// </summary>
        /// <returns></returns>
        public override Task<TSubGridRequestsResponse> ExecuteAsync()
        {
            PrepareForExecution();

            // Construct the function to be used
            var func = new SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>();

            return Compute.BroadcastAsync(func, arg)
              .ContinueWith(result => result.Result.Aggregate((first, second) => (TSubGridRequestsResponse) first.AggregateWith(second)))
              .ContinueWith(x => 
              {
                Log.LogInformation($"SubGridRequests.ExecuteAsync() for DM:{TRexTask.PipeLine.DataModelID} from node {TRexTask.TRexNodeID} for data type {TRexTask.GridDataType}");
                return x.WaitAndUnwrapException();
              });
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}

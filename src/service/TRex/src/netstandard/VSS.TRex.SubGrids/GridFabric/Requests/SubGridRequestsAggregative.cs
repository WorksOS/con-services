using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Responses;

namespace VSS.TRex.SubGrids.GridFabric.Requests
{
    /// <summary>
    /// Performs sub grid requests where the processing result is aggregated and returned as one of set of partitioned responses
    /// from the cache compute cluster
    /// </summary>
    public class SubGridRequestsAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRequestsAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

        /// <summary>
        /// Default no-arg constructor that delegates construction to the base class
        /// </summary>
        public SubGridRequestsAggregative()
        {
        }

        /// <summary>
        /// This needs to implement IDisposable because the sub grid requestor layer wants to use 'using' for the progressive
        /// sub grid requestor. This means the aggregative one also need to implement IDisposable, even though it has no behaviour.
        /// </summary>
        public void Dispose()
        {
        }
     
        /// <summary>
        /// Overrides the base Execute() semantics to add a listener available for aggregated processing of sub grids in the request engine.
        /// </summary>
        /// <returns></returns>
        public override TSubGridRequestsResponse Execute()
        {
            CheckArguments();

            // Construct the argument to be supplied to the compute cluster
            PrepareArgument();

            Log.LogInformation($"Prepared argument has TRexNodeId = {arg.TRexNodeID}");
            Log.LogInformation($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} sub grids");
            Log.LogInformation($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()} sub grids");

            TSubGridRequestsResponse taskResult = null;

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                // Construct the function to be used
                var func = new SubGridsRequestComputeFuncAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>(TRexTask);
                
                // Invoke it
                taskResult = func.Invoke(arg);
            }
            finally
            {
                sw.Stop();
                Log.LogInformation($"TaskResult {(taskResult == null ? "<NullResult>" : taskResult.ResponseCode.ToString())}: SubGridRequests.Execute() for DM:{TRexTask.PipeLine.DataModelID} from node {TRexTask.TRexNodeID} for data type {TRexTask.GridDataType} took {sw.ElapsedMilliseconds}ms");
            }

            // Advise the pipeline of all the sub grids that were examined in the aggregated processing
            TRexTask.PipeLine.SubGridsProcessed(taskResult?.NumSubgridsExamined ?? 0);

            // Notify the pipeline that all processing has been completed for it
            TRexTask.PipeLine.PipelineCompleted = true;

            // Send the appropriate response to the caller
            return taskResult;
        }

        /// <summary>
        /// Overrides the base Execute() semantics to add a listener available for aggregated processing of sub grids in the request engine.
        /// </summary>
        /// <returns></returns>
        public override Task<TSubGridRequestsResponse> ExecuteAsync()
        {
            CheckArguments();
         
            // Construct the argument to be supplied to the compute cluster
            PrepareArgument();
         
            Log.LogInformation($"Prepared argument has TRexNodeId = {arg.TRexNodeID}");
            Log.LogInformation($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} sub grids");
            Log.LogInformation($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()} sub grids");
                
            // Construct the function to be used
            IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse> func = new SubGridsRequestComputeFuncAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>(TRexTask);

            // Invoke it
            return Task.Run(() => func.Invoke(arg)).ContinueWith(result =>
            {        
                // Advise the pipeline of all the sub grids that were examined in the aggregated processing
                TRexTask.PipeLine.SubGridsProcessed(result.Result?.NumSubgridsExamined ?? 0);
           
               // Notify the pipeline that all processing has been completed for it
               TRexTask.PipeLine.PipelineCompleted = true;
           
               return result.Result;
            });
        }
  }
}

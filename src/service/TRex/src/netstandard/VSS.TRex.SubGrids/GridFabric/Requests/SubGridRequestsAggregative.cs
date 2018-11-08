using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;

namespace VSS.TRex.SubGrids.GridFabric.Requests
{
    /// <summary>
    /// Performs subgrid requests where the processing result is aggregated and returned as one of set of partitioned responses
    /// from the cache compute cluster
    /// </summary>
    public class SubGridRequestsAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse> 
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Default no-arg constructor that delegates construction to the base class
        /// </summary>
        public SubGridRequestsAggregative()
        {
        }

        /// <summary>
        /// Overrides the base Execute() semantics to add a listener available for aggregated processing of subgrids in the request engine.
        /// </summary>
        /// <returns></returns>
        public override ICollection<TSubGridRequestsResponse> Execute()
        {
            CheckArguments();

            // Construct the argument to be supplied to the compute cluster
            PrepareArgument();

            Log.LogInformation($"Prepared argument has TRexNodeId = {arg.TRexNodeID}");
            Log.LogInformation($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} subgrids");
            Log.LogInformation($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()} subgrids");

            TSubGridRequestsResponse taskResult = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                // Construct the function to be used
                IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse> func = new SubGridsRequestComputeFuncAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>
                {
                    Task = Task
                };
                
                // Invoke it
                taskResult = func.Invoke(arg);
            }
            finally
            {
                sw.Stop();
                Log.LogInformation($"TaskResult {(taskResult == null ? "<NullResult>" : taskResult.ResponseCode.ToString())}: SubgridRequests.Execute() for DM:{Task.PipeLine.DataModelID} from node {Task.TRexNodeID} for data type {Task.GridDataType} took {sw.ElapsedMilliseconds}ms");
            }

            // Advise the pipeline of all the subgrids that were examined in the aggregated processing
            Task.PipeLine.SubgridsProcessed(taskResult.NumSubgridsExamined);

            // Notify the pipeline that all processing has been completed for it
            Task.PipeLine.PipelineCompleted = true;

            // Send the appropriate response to the caller
            return new List<TSubGridRequestsResponse>() { taskResult };
        }
    }
}

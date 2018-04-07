using Apache.Ignite.Core.Compute;
using log4net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Performs subgrid requests where the procesing result is aggregated and returned as one of set of partitioned responses
    /// from the cache compute cluster
    /// </summary>
    public class SubGridRequestsAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse> 
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default no-arg constructor that delegates construction to the base class
        /// </summary>
        public SubGridRequestsAggregative()
        {
        }

        /// <summary>
        /// Overrides the base Execute() semantics to add a listener available for aggregative processing of subgrids in the request engine.
        /// </summary>
        /// <returns></returns>
        public override ICollection<TSubGridRequestsResponse> Execute()
        {
            CheckArguments();

            // Construct the argument to be supplied to the compute cluster
            PrepareArgument();

            Log.Info($"Prepared argument has RaptorNodeID = {arg.RaptorNodeID}");
            Log.Info($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} subgrids");
            Log.Info($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()} subgrids");

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
                Log.InfoFormat("TaskResult {0}: SubgidRequests.Execute() for DM:{1} from node {2} for data type {3} took {4}ms",
                               taskResult == null ? "<NullResult>" : taskResult.ResponseCode.ToString(), 
                               Task.PipeLine.DataModelID, Task.RaptorNodeID, Task.GridDataType, sw.ElapsedMilliseconds);
            }

            // Advise the pipeline of all the subgrids that were examined in the aggregative processing
            Task.PipeLine.SubgridsProcessed(taskResult.NumSubgridsExamined);

            // Notify the pipline that all processing has been completed for it
            Task.PipeLine.PipelineCompleted = true;

            // Send the appropriate response to the caller
            return new List<TSubGridRequestsResponse>() { taskResult };
        }
    }
}

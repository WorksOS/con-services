using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Listeners;
using VSS.VisionLink.Raptor.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Requests subgrids from the cache compute cluster allowing in-progress updates of results to be sent back to
    /// the calling context via a subgrid listener for processing.
    /// </summary>
    public class SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The listener to which the processing mengine may send in-progress updates during processing of the overall subgrids request
        /// </summary>
        public SubGridListener Listener { get; set; } = null;

        /// <summary>
        /// Default no-arg constructor thje delgates construction to the base class
        /// </summary>
        public SubGridRequestsProgressive() : base()
        {
        }

        /// <summary>
        /// Creates the subgrid listener on the MessageTopic defined in th eargument to be sent to the cache cluster
        /// </summary>
        private void CreateSubGridListener()
        {
            // Create any required listener for periodic responses directly sent from the processing context to this context
            if (!String.IsNullOrEmpty(arg.MessageTopic))
            {
                Listener = new SubGridListener(Task);

                // Create a messaging group the cluster can use to send messages back to and establish a local listener
                var msgGroup = _Compute.ClusterGroup.GetMessaging();
                msgGroup.LocalListen(Listener, arg.MessageTopic);
            }
        }

        /// <summary>
        /// Overrides the base Execut() semantics to add a listener available for in-progress updates of information
        /// from the processing engine.
        /// </summary>
        /// <returns></returns>
        public override ICollection<TSubGridRequestsResponse> Execute()
        {
            CheckArguments();

            // Construct the argument to be supplied to the compute cluster
            PrepareArgument();

            Log.Info($"Prepared argument has RaptorNodeID = {arg.RaptorNodeID}");
            Log.Info($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} subgrids");
            Log.Info($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()}");

            Task<ICollection<TSubGridRequestsResponse>> taskResult = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                CreateSubGridListener();

                // Construct the function to be used
                IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse> func = new SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>();

                // Note: Broadcast will block until all compute nodes receiving the request have responded, or
                // until the internal Ignite timeout expires
                //result = _compute.Broadcast(func, arg);

                taskResult = _Compute.BroadcastAsync(func, arg);
                taskResult.Wait(30000);
            }
            finally
            {
                sw.Stop();
                Log.InfoFormat("TaskResult {0}: SubgidRequests.Execute() for DM:{1} from node {2} for data type {3} took {4}ms",
                               taskResult.Status, Task.PipeLine.DataModelID, Task.RaptorNodeID, Task.GridDataType, sw.ElapsedMilliseconds);
            }

            // Notify the pipline that all processing has been completed for it
            //Task.PipeLine.PipelineCompleted = true;

            // Send the appropriate response to the caller
            //return result;
            return taskResult.Result;
        }
    }
}

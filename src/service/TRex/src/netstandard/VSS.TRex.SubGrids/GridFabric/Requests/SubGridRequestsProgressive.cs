using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.GridFabric.Listeners;

namespace VSS.TRex.SubGrids.GridFabric.Requests
{
    /// <summary>
    /// Requests subgrids from the cache compute cluster allowing in-progress updates of results to be sent back to
    /// the calling context via a subgrid listener for processing.
    /// </summary>
    public class SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, IAggregateWith<TSubGridRequestsResponse>, new()
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

        /// <summary>
        /// The listener to which the processing engine may send in-progress updates during processing of the overall subgrids request
        /// </summary>
        private SubGridListener Listener { get; set; }

        /// <summary>
        /// The MsgGroup into which the listener has been added
        /// </summary>
        private IMessaging MsgGroup { get; set; }

        /// <summary>
        /// Default no-arg constructor that delegates construction to the base class
        /// </summary>
        public SubGridRequestsProgressive()
        {
        }

        /// <summary>
        /// Creates the subgrid listener on the MessageTopic defined in the argument to be sent to the cache cluster
        /// </summary>
        private void CreateSubGridListener()
        {
            // Create any required listener for periodic responses directly sent from the processing context to this context
            if (!string.IsNullOrEmpty(arg.MessageTopic))
            {
                Listener = new SubGridListener(TRexTask);

                StartListening();
            }
        }

        public void StartListening()
        {
            if (MsgGroup == null)
            {
                // Create a messaging group the cluster can use to send messages back to and establish a local listener
                MsgGroup = _Compute.ClusterGroup.GetMessaging();
                MsgGroup.LocalListen(Listener, arg.MessageTopic);
            }
        }

        public void StopListening()
        {
            // De-register the listener from the message group
            MsgGroup?.StopLocalListen(Listener);

            MsgGroup = null;
        }

        private void PrepareForExecution()
        {
          CheckArguments();

          // Construct the argument to be supplied to the compute cluster
          PrepareArgument();

          Log.LogInformation($"Prepared argument has TRexNodeId = {arg.TRexNodeID}");
          Log.LogInformation($"Production Data mask in argument to renderer contains {ProdDataMask.CountBits()} subgrids");
          Log.LogInformation($"Surveyed Surface mask in argument to renderer contains {SurveyedSurfaceOnlyMask.CountBits()} subgrids");

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

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                // Construct the function to be used
                IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse> func = new SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>();

                taskResult = _Compute.BroadcastAsync(func, arg);
                taskResult.Wait(30000);
            }
            finally
            {
                sw.Stop();
                Log.LogInformation($"TaskResult {taskResult?.Status}: SubgridRequests.Execute() for DM:{TRexTask.PipeLine.DataModelID} from node {TRexTask.TRexNodeID} for data type {TRexTask.GridDataType} took {sw.ElapsedMilliseconds}ms");
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
            IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse> func = new SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>();

            return _Compute.BroadcastAsync(func, arg)
              .ContinueWith(result => result.Result.Aggregate((first, second) => (TSubGridRequestsResponse) first.AggregateWith(second)))
              .ContinueWith(x =>
              {
                Log.LogInformation($"SubgridRequests.Execute() for DM:{TRexTask.PipeLine.DataModelID} from node {TRexTask.TRexNodeID} for data type {TRexTask.GridDataType}");
                return x.Result;
              });
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopListening();
                }

                disposedValue = true;
            }
        }

        // Note: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SubGridRequestsProgressive() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // Note: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

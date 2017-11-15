using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Designs.GridFabric.Requests;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Listeners;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// The SubGridRequests GridFabric class sends a request to the grid for a collection of subgrids to be processed according 
    /// to relevant filters other parameters. The grid fabric responds with responses as the servers in the fabric compute them, sending
    /// them to the Raptor node identified by the RaptorNodeID property
    /// </summary>
    public class SubGridRequests : CacheComputePoolRequest
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Task is the business logic that will handle the response to the subgrids request
        /// </summary>
        public ITask Task = null;

        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public long SiteModelID { get; set; } = -1;

        /// <summary>
        /// The request ID assigned to the activity requiring these subgrids to be requested. This ID is used to funnel 
        /// traffic from the processing cluster into the correct processing context
        /// </summary>
        public long RequestID { get; set; } = -1;

        /// <summary>
        /// The identifier of the Raptor Node that is issuing the request for subgrids and which wants to receive the processed
        /// subgrid responses
        /// </summary>
        public string RaptorNodeID { get; set; } = String.Empty;

        /// <summary>
        /// The type of grid data to be retrieved from the subgrid requests
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// A subgrid bit mask tree identifying all the subgrids that require processing
        /// </summary>
        public SubGridTreeBitMask Mask { get; set; } = null;

        /// <summary>
        /// The set of filters to be applied to the subgrids being processed
        /// </summary>
        public FilterSet Filters { get; set; } = null;

        /// <summary>
        /// Denotes whether results of these requests should include any surveyed surfaces in the site model
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; } = false;

        /// <summary>
        /// No arg constructor that establishes this request as a cache compute request
        /// </summary>
        public SubGridRequests() : base()
        {
        }

        /// <summary>
        /// Constructor accepting the mask of subgrids to request and the filters that apply to them
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="Filters"></param>
        public SubGridRequests(ITask task, 
                               long siteModelID, 
                               long requestID, 
                               string raptorNodeID, 
                               GridDataType requestedGridDataType, 
                               bool includeSurveyedSurfaceInformation,
                               SubGridTreeBitMask mask, 
                               FilterSet filters) : this()
        {
            Task = task;
            SiteModelID = siteModelID;
            RequestID = requestID;
            RequestedGridDataType = requestedGridDataType;
            IncludeSurveyedSurfaceInformation = includeSurveyedSurfaceInformation;
            Mask = mask;
            Filters = filters;
            RaptorNodeID = raptorNodeID;
        }

        private SubGridsRequestArgument PrepareArgument()
        {
            MemoryStream MS = new MemoryStream();

            SubGridTreePersistor.Write(Mask, new BinaryWriter(MS, Encoding.UTF8, true));
            MS.Position = 0;
             
            return new SubGridsRequestArgument(SiteModelID, 
                                               RequestID, 
                                               RequestedGridDataType,
                                               IncludeSurveyedSurfaceInformation,
                                               MS, 
                                               Filters, 
                                               String.Format("SubGridRequest:{0}", RequestID), 
                                               RaptorNodeID);
        }

        /// <summary>
        /// Executes a request for a number of subgrids to be processed according to filters and other
        /// parameters
        /// </summary>
        /// <returns></returns>
        public ICollection<SubGridRequestsResponse> Execute()
        {
            // Make sure things look kosher
            if (Mask == null || Filters == null || RequestID == -1)
            {
                throw new ArgumentException("Mask, Filters or RequestID not initialised");
            }

            Log.InfoFormat("Preparing argument with RaptorNodeID = {0}", RaptorNodeID);

            // Construct the argument to be supplied to the compute cluster
            SubGridsRequestArgument arg = PrepareArgument();

            Log.InfoFormat("Prepared argument has RaptorNodeID = {0}", arg.RaptorNodeID);
            Log.Info(String.Format("Mask in argument to renderer contains {0} subgrids", Mask.CountBits()));

            // Construct the function to be used
            IComputeFunc<SubGridsRequestArgument, SubGridRequestsResponse> func = new SubGridsRequestComputeFunc();

            // Create a messaging group the cluster can use to send messages back to and establish
            // a local listener
            var msgGroup = _compute.ClusterGroup.GetMessaging();
            msgGroup.LocalListen(new SubGridListener(Task), arg.MessageTopic);

            // Note: Broadcast will block until all compute nodes receiving the request have responded, or
            // until the internal Ignite timeout expires
            //ICollection<SubGridRequestsResponse> result = compute.Broadcast(func, arg);
            Task<ICollection<SubGridRequestsResponse>> taskResult = _compute.BroadcastAsync(func, arg);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                taskResult.Wait(120000);
            }
            finally
            {
                sw.Stop();
                Log.InfoFormat("TaskResult {0}: SubgidRequests.Execute() for DM:{1} from node {2} for data type {3} took {4}ms", 
                               taskResult.Status, Task.PipeLine.DataModelID, Task.RaptorNodeID, Task.GridDataType, sw.ElapsedMilliseconds);
            }

            // Send the appropriate response to the caller
            // return result;
            return taskResult.Result;
        }
    }
}

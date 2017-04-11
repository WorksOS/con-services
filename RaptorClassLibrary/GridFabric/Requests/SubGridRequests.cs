using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Listeners;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// The SubGridRequests GridFabric class sends a request to the grid for a collection of subgrids to be processed according 
    /// to relevant filters other parameters. The grid fabric responds with responses as the servers in the fabric compute them
    /// </summary>
    public class SubGridRequests
    {
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
        /// The type of grid data to be retrieved from the subgrid requests
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// A subgrid bit mask tree identifying all the subgrids that require processing
        /// </summary>
        public SubGridTreeBitMask Mask { get; set; } = null;

        /// <summary>
        /// The set of filters to be applied to the suibgrids being processed
        /// </summary>
        public FilterSet Filters { get; set; } = null;

        /// <summary>
        /// No arg constructor
        /// </summary>
        public SubGridRequests()
        {
        }

        /// <summary>
        /// Constructor accepting the mask of subgrids to request and the filters that apply to them
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="Filters"></param>
        public SubGridRequests(ITask task, long siteModelID, long requestID, GridDataType requestedGridDataType, SubGridTreeBitMask mask, FilterSet filters) : this()
        {
            Task = task;
            SiteModelID = siteModelID;
            RequestID = requestID;
            RequestedGridDataType = requestedGridDataType;
            Mask = mask;
            Filters = filters;
        }

        private SubGridsRequestArgument PrepareArgument()
        {
            MemoryStream MS = new MemoryStream();

            SubGridTreePersistor.Write(Mask, new BinaryWriter(MS));
            MS.Position = 0;
             
            return new SubGridsRequestArgument(SiteModelID, RequestID, RequestedGridDataType, MS, String.Format("SubGridRequest:{0}", RequestID));
        }

        /// <summary>
        /// Executes a request for a number of subgrids to be processed according to filters and other
        /// parameters
        /// </summary>
        /// <returns></returns>
        public ICollection<SubGridRequestsResponse> Execute()
        {
            // Make sure things look koscher
            if (Mask == null || Filters == null || RequestID == -1)
            {
                throw new ArgumentException("Mask, Filters or RequestID not initialised");
            }

            // Construct the argument to be supplied to the compute cluster
            SubGridsRequestArgument arg = PrepareArgument();

            Console.WriteLine("Mask in argument to renderer contains {0} subgrids", Mask.CountBits());

            // Construct the function to be used
            IComputeFunc<SubGridsRequestArgument, SubGridRequestsResponse> func = new SubGridsRequestComputeFunc();

            // Get a reference to the Ignite cluster
            IIgnite ignite = Ignition.GetIgnite("Raptor");

            // Create a messaging group the cluster can use to send messages back to and establish
            // a local listener
            var msgGroup = ignite.GetCompute().ClusterGroup.GetMessaging();
            msgGroup.LocalListen(new SubGridListener(Task), arg.MessageTopic);

            // Get a reference to the compute cluster group and send the request to it for processing
            // Note: Broadcast will block until all compute nodes receiving the request have responded, or
            // until the internal Ignite timeout expires
            ICompute group = ignite.GetCompute();
            ICollection<SubGridRequestsResponse> result = group.Broadcast(func, arg);

            // Send the appropriate response to the caller
            return result;
        }
    }
}

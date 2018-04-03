using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Pipelines.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
{
    /// <summary>
    /// ITaskBase/TaskBase is an interface/base class implementation that other classes 
    /// may extend to provide specific handling logic for responses from queries, such as 
    /// subgrids and profile sections that require additional processing to arrive at 
    /// the final result (such as a rendered tile)
    /// </summary>
    public abstract class TaskBase : ITask
    {
        /// <summary>
        /// The request descriptor assigned to the task.
        /// </summary>
        public long RequestDescriptor = -1;

        /// <summary>
        /// Determines if the processing of the task activities as been cencelled by external control
        /// </summary>
        public bool IsCancelled = false;

        /// <summary>
        /// The type of grid data being processed by this task
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// The raptor node wanting to recieve the results of task bases subgrid requests to the PSNode clustered processing layer
        /// </summary>
        public string RaptorNodeID { get; set; } = string.Empty;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TaskBase()
        {
        }

        /// <summary>
        /// Constructor accepting a request descriptor identifying the overall request this task is associated with
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="raptorNodeID"></param>
        /// <param name="gridDataType"></param>
        public TaskBase(long requestDescriptor, string raptorNodeID, GridDataType gridDataType)
        {
            RequestDescriptor = requestDescriptor;
            RaptorNodeID = raptorNodeID;
            GridDataType = gridDataType;
        }

        /// <summary>
        /// TransferReponse is the sink for responses received from the processing layers.
        /// </summary>
        /// <param name="response"></param>
        public abstract bool TransferResponse(object response);

        /// <summary>
        /// TransferReponses is the sink for sets of responses received from the processing layers.
        /// </summary>
        /// <param name="responses"></param>
        public abstract bool TransferResponses(object [] responses);

        /// <summary>
        /// Cancel sets the cancelled flag to true for the processing engine to take note of and 
        /// take any required actions to cancel an active request.
        /// </summary>
        public virtual void Cancel() => IsCancelled = true;

        /// <summary>
        /// A reference to a subgrid processing pipeline associated with this task
        /// </summary>
        public ISubGridPipelineBase PipeLine { get; set; } = null;
    }
}

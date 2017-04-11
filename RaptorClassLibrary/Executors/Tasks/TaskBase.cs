using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
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
        public long RequestDescriptor = -1;
        public bool IsCancelled = false;
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// Constructor accepting a request descriptor identifying the overall request this task is associated with
        /// </summary>
        /// <param name="requestDescriptor"></param>
        public TaskBase(long requestDescriptor, GridDataType gridDataType)
        {
            RequestDescriptor = requestDescriptor;
            GridDataType = gridDataType;
        }

        /// <summary>
        /// TransferReponse is the sink for responses received from the processing layers.
        /// </summary>
        /// <param name="response"></param>
        public abstract bool TransferResponse(object response);

        /// <summary>
        /// Cancel sets the cancelled flag to true for the processing engine to take note of and 
        /// take any required actions to cancel an active request.
        /// </summary>
        public virtual void Cancel() => IsCancelled = true;
    }
}

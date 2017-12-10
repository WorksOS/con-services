using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Listeners;
using VSS.VisionLink.Raptor.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Requests subgrids from the cache compute cluster allowing in-progress updates of results to be sent back to
    /// the calling context via a subgrid listener for processing.
    /// </summary>
    public class SubGridRequestsProgressive : SubGridRequestsBase<SubGridsRequestComputeFuncProgressive>
    {
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
        public override ICollection<SubGridRequestsResponse> Execute()
        {
            PrepareArgument();
            CreateSubGridListener();

            return base.Execute();
        }
    }
}

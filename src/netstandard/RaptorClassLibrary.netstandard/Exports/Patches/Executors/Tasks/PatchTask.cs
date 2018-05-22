using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Executors.Tasks;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.Executors.Tasks
{
    /// <summary>
    /// The task responsible for receiving subgrids to be aggregated into a Patch response
    /// </summary>
    public class PatchTask : PipelinedSubGridTask
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The collection of subgrids being collected for a patch response
        /// </summary>
        public List<IClientLeafSubGrid> PatchSubgrids = new List<IClientLeafSubGrid>();

        /// <summary>
        /// Constructs the patch task
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="tRexNodeId"></param>
        /// <param name="gridDataType"></param>
        public PatchTask(long requestDescriptor, 
                         string tRexNodeId, 
                         GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
        {
        }

        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            if (!base.TransferResponse(response))
            {
                Log.LogWarning("Base TransferResponse returned false");
                return false;
            }

            PatchSubgrids.Add((response as IClientLeafSubGrid[])[0]);

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.Compute;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.TAGFiles.Executors;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
    public class SubmitTAGFileComputeFunc : BaseRaptorComputeFunc, IComputeFunc<SubmitTAGFileRequestArgument, SubmitTAGFileResponse>
    {
        /// <summary>
        /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
        /// </summary>
        public SubmitTAGFileComputeFunc() : base(RaptorGrids.RaptorMutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
        {
        }

        /// <summary>
        /// The Invoke method for the compute func - calls the TAG file processing executor to do the work
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SubmitTAGFileResponse Invoke(SubmitTAGFileRequestArgument arg)
        {
            return SubmitTAGFileExecutor.Execute(arg.ProjectID, arg.AssetID, arg.TAGFileName, arg.TagFileContent);
        }
    }
}

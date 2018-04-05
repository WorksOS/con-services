using Apache.Ignite.Core.Compute;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.TAGFiles.Executors;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.TAGFiles.GridFabric.ComputeFuncs
{
    public class ProcessTAGFileComputeFunc : BaseRaptorComputeFunc, IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
    {
        /// <summary>
        /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
        /// </summary>
        public ProcessTAGFileComputeFunc() : base(RaptorGrids.RaptorImmutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
        {
        }

        /// <summary>
        /// The Invoke method for the compute func - calls the TAG file processing executor to do the work
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public ProcessTAGFileResponse Invoke(ProcessTAGFileRequestArgument arg)
        {
            return ProcessTAGFilesExecutor.Execute(arg.ProjectID, arg.AssetID, arg.TAGFiles);
        }
    }
}

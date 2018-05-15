using Apache.Ignite.Core.Compute;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
    public class ProcessTAGFileComputeFunc : BaseRaptorComputeFunc, IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
    {
        /// <summary>
        /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
        /// </summary>
        public ProcessTAGFileComputeFunc() : base(RaptorGrids.RaptorMutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
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

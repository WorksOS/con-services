using Apache.Ignite.Core.Compute;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.TAGFiles.GridFabric.Requests
{
    public class ProcessTAGFileRequest : TAGFileProcessingPoolRequest
    {
        /// <summary>
        /// Processes a set of TAG files from a machine into a project
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override ProcessTAGFileResponse Execute(ProcessTAGFileRequestArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse> func = new ProcessTAGFileComputeFunc();

            Task<ProcessTAGFileResponse> taskResult = _Compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}

using Apache.Ignite.Core.Compute;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.Requests
{
    /// <summary>
    /// Provides a request to process one or more TAG files into a project
    /// </summary>
    public class ProcessTAGFileRequest : TAGFileProcessingPoolRequest<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
    {
        /// <summary>
        /// Local reference to the compute func used to execute the processing request on the grid.
        /// </summary>
        private IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse> func;

        /// <summary>
        /// No-arg constructor that creates a default TAG file submission request with a singleton ConputeFunc
        /// </summary>
        public ProcessTAGFileRequest()
        {
            // Construct the function to be used
            func = new ProcessTAGFileComputeFunc();
        }

        /// <summary>
        /// Processes a set of TAG files from a machine into a project
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override ProcessTAGFileResponse Execute(ProcessTAGFileRequestArgument arg)
        {
            Task<ProcessTAGFileResponse> taskResult = _Compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}

using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.TRex.TAGFiles.GridFabric.Requests
{
    /// <summary>
    /// Supports submitting a single TAG file to be considered for processing depending on TAG File Auhthorization checks.
    /// </summary>
    public class SubmitTAGFileRequest : TAGFileProcessingPoolRequest<SubmitTAGFileRequestArgument, SubmitTAGFileResponse>
    {
        /// <summary>
        /// Local reference to the compute func used to execute the submission request on the grid.
        /// </summary>
        private IComputeFunc<SubmitTAGFileRequestArgument, SubmitTAGFileResponse> func;

        /// <summary>
        /// No-arg constructor that creates a default TAG file submission request with a singleton ConputeFunc
        /// </summary>
        public SubmitTAGFileRequest()
        {
            // Construct the function to be used
            func = new SubmitTAGFileComputeFunc();
        }

        /// <summary>
        /// Processes a set of TAG files from a machine into a project
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override SubmitTAGFileResponse Execute(SubmitTAGFileRequestArgument arg)
        {
            Task<SubmitTAGFileResponse> taskResult = _Compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}

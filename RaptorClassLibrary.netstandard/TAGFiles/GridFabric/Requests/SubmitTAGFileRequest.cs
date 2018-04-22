using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.TRex.TAGFiles.GridFabric.Requests
{
    public class SubmitTAGFileRequest : TAGFileProcessingPoolRequest<SubmitTAGFileRequestArgument, SubmitTAGFileResponse>
    {
        /// <summary>
        /// Processes a set of TAG files from a machine into a project
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override SubmitTAGFileResponse Execute(SubmitTAGFileRequestArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<SubmitTAGFileRequestArgument, SubmitTAGFileResponse> func = new SubmitTAGFileComputeFunc();

            Task<SubmitTAGFileResponse> taskResult = _Compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}

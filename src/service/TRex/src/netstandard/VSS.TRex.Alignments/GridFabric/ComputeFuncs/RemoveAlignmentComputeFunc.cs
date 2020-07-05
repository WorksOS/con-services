using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments.Executors;
using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Alignments.GridFabric.ComputeFuncs
{
  public class RemoveAlignmentComputeFunc : BaseComputeFunc, IComputeFunc<RemoveAlignmentArgument, RemoveAlignmentResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RemoveAlignmentComputeFunc>();

    public RemoveAlignmentResponse Invoke(RemoveAlignmentArgument arg)
    {
      try
      {
        // Remove the design from the project
        var executor = new RemoveAlignmentExecutor();
        var result = executor.Execute(arg.ProjectID, arg.AlignmentID);

        return new RemoveAlignmentResponse
        {
          DesignUid = arg.AlignmentID,
          RequestResult = result
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception: ");
        return null;
      }
    }
  }
}

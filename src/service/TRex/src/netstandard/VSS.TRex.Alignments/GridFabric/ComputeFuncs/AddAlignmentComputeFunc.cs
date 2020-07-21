using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments.Executors;
using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Alignments.GridFabric.ComputeFuncs
{
  public class AddAlignmentComputeFunc : BaseComputeFunc, IComputeFunc<AddAlignmentArgument, AddAlignmentResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddAlignmentComputeFunc>();

    public AddAlignmentResponse Invoke(AddAlignmentArgument arg)
    {
      try
      {
        // Add the new alignment to the project
        var executor = new AddAlignmentExecutor();
        var design = executor.Execute(arg.ProjectID, arg.DesignDescriptor, arg.Extents);

        if (design == null)
        {
          return new AddAlignmentResponse
          {
            AlignmentUid = Guid.Empty,
            RequestResult = DesignProfilerRequestResult.FailedToAddDesign
          };
        }

        return new AddAlignmentResponse
        {
          AlignmentUid = design.ID,
          RequestResult = DesignProfilerRequestResult.OK,
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception adding alignment");

        return new AddAlignmentResponse()
        {
          AlignmentUid = Guid.Empty,
          RequestResult = DesignProfilerRequestResult.UnknownError
        };
      }
    }
  }
}

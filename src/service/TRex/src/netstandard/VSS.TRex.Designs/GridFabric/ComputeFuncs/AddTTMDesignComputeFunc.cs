using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class AddTTMDesignComputeFunc : BaseComputeFunc, IComputeFunc<AddTTMDesignArgument, AddTTMDesignResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddTTMDesignComputeFunc>();

    public AddTTMDesignResponse Invoke(AddTTMDesignArgument arg)
    {
      try
      {
        // Add the new design to the project
        var executor = new AddTTMDesignExecutor();
        var design = executor.Execute(arg.ProjectID, arg.DesignDescriptor, arg.Extents, arg.ExistenceMap);

        if (design == null)
        {
          return new AddTTMDesignResponse
          {
            DesignUid = Guid.Empty,
            RequestResult = DesignProfilerRequestResult.FailedToAddDesign
          };
        }

        return new AddTTMDesignResponse
        {
          DesignUid = design.ID,
          RequestResult = DesignProfilerRequestResult.OK,
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

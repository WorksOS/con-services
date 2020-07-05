using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class RemoveTTMDesignComputeFunc : BaseComputeFunc, IComputeFunc<RemoveTTMDesignArgument, RemoveTTMDesignResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RemoveTTMDesignComputeFunc>();

    public RemoveTTMDesignResponse Invoke(RemoveTTMDesignArgument arg)
    {
      try
      {
        // Remove the design from the project
        var executor = new RemoveTTMDesignExecutor();
        var result = executor.Execute(arg.ProjectID, arg.DesignID);

        return new RemoveTTMDesignResponse
        {
          DesignUid = arg.DesignID,
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

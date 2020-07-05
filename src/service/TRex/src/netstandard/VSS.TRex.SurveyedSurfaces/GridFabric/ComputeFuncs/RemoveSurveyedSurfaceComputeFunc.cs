using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurface.GridFabric.Responses;
using VSS.TRex.SurveyedSurfaces.Executors;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs
{
  public class RemoveSurveyedSurfaceComputeFunc : BaseComputeFunc, IComputeFunc<RemoveSurveyedSurfaceArgument, RemoveSurveyedSurfaceResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RemoveSurveyedSurfaceComputeFunc>();

    public RemoveSurveyedSurfaceResponse Invoke(RemoveSurveyedSurfaceArgument arg)
    {
      try
      {
        // Remove the surveyed surface from the project
        var executor = new RemoveSurveyedSurfaceExecutor();
        var result = executor.Execute(arg.ProjectID, arg.DesignID);

        return new RemoveSurveyedSurfaceResponse
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

using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.Executors;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.Responses;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs
{
  public class AddSurveyedSurfaceComputeFunc : BaseComputeFunc, IComputeFunc<AddSurveyedSurfaceArgument, AddSurveyedSurfaceResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddSurveyedSurfaceComputeFunc>();

    public AddSurveyedSurfaceResponse Invoke(AddSurveyedSurfaceArgument arg)
    {
      try
      {
        // Add the new design to the project
        var executor = new AddSurveyedSurfaceExecutor();
        var design = executor.Execute(arg.ProjectID, arg.DesignDescriptor, arg.AsAtDate, arg.Extents, arg.ExistenceMap);

        if (design == null)
        {
          return new AddSurveyedSurfaceResponse
          {
            DesignUid = Guid.Empty,
            RequestResult = DesignProfilerRequestResult.FailedToAddDesign
          };
        }

        return new AddSurveyedSurfaceResponse
        {
          DesignUid = design.ID,
          RequestResult = DesignProfilerRequestResult.OK,
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception adding surveyed surface");
        return new AddSurveyedSurfaceResponse { RequestResult = DesignProfilerRequestResult.UnknownError };
      }
    }
  }
}

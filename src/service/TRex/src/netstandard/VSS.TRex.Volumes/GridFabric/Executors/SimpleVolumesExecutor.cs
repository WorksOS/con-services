using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Executors
{
  public class SimpleVolumesExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SimpleVolumesExecutor>();

    private SimpleVolumesResponse ConvertBoundaryFromGridToWGS84(Guid projectUid, SimpleVolumesResponse response)
    {
      if (!(response.BoundingExtentGrid.IsValidPlanExtent))
        return response; // No conversion possible

      var NEECoords = new[]
      {
        new XYZ(response.BoundingExtentGrid.MinX, response.BoundingExtentGrid.MinY),
        new XYZ(response.BoundingExtentGrid.MaxX, response.BoundingExtentGrid.MaxY)
      };

      (var errorCode, XYZ[] LLHCoords) = ConvertCoordinates.NEEToLLH(DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid).CSIB(), NEECoords);

      if (errorCode == RequestErrorStatus.OK)
      {
        response.BoundingExtentLLH = new BoundingWorldExtent3D
        {
          MinX = MathUtilities.RadiansToDegrees(LLHCoords[0].X),
          MinY = MathUtilities.RadiansToDegrees(LLHCoords[0].Y),
          MaxX = MathUtilities.RadiansToDegrees(LLHCoords[1].X),
          MaxY = MathUtilities.RadiansToDegrees(LLHCoords[1].Y)
        };
      }
      else
      {
        Log.LogInformation("Summary volume failure, could not convert bounding area from grid to WGS coordinates");
        response.ResponseCode = SubGridRequestsResponseResult.Failure;
      }

      return response;
    }

    public SimpleVolumesResponse Execute(SimpleVolumesRequestArgument arg)
    {
      SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

      Log.LogInformation("Executing SimpleVolumesRequestComputeFunc_ApplicationService.Execute()");

      // Calculate the volumes and convert the grid bounding rectangle into WGS 84 lat/long to return to the caller.
      return ConvertBoundaryFromGridToWGS84(arg.ProjectID, request.Execute(arg));
    }
  }
}

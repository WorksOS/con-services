using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Executors
{
  public class SimpleVolumesExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SimpleVolumesExecutor>();

    private static async Task<SimpleVolumesResponse> ConvertBoundaryFromGridToWGS84(Guid projectUid, SimpleVolumesResponse response)
    {
      if (!response.BoundingExtentGrid.IsValidPlanExtent)
        return response; // No conversion possible

      var neeCoords = new[]
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        new XYZ(response.BoundingExtentGrid.MinX, response.BoundingExtentGrid.MinY,
              response.BoundingExtentGrid.MinZ == Consts.NullDouble ? 0.0 : response.BoundingExtentGrid.MinZ),
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        new XYZ(response.BoundingExtentGrid.MaxX, response.BoundingExtentGrid.MaxY,
              response.BoundingExtentGrid.MaxZ == Consts.NullDouble ? 0.0 : response.BoundingExtentGrid.MaxZ)
      };

      var (errorCode, llhCoords) = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid).CSIB(), neeCoords);

      if (errorCode == RequestErrorStatus.OK)
      {
        response.BoundingExtentLLH = new BoundingWorldExtent3D
        {
          MinX = MathUtilities.RadiansToDegrees(llhCoords[0].X),
          MinY = MathUtilities.RadiansToDegrees(llhCoords[0].Y),
          MaxX = MathUtilities.RadiansToDegrees(llhCoords[1].X),
          MaxY = MathUtilities.RadiansToDegrees(llhCoords[1].Y)
        };
      }
      else
      {
        _log.LogInformation($"Summary volume failure, could not convert bounding area from grid to WGS coordinates, bounding extents = [{response.BoundingExtentGrid}], error = {errorCode}");
        response.ResponseCode = SubGridRequestsResponseResult.Failure;
      }

      return response;
    }

    public async Task<SimpleVolumesResponse> ExecuteAsync(SimpleVolumesRequestArgument arg)
    {
      var request = new SimpleVolumesRequest_ClusterCompute();

      _log.LogInformation("Executing SimpleVolumesRequestComputeFunc_ApplicationService.ExecuteAsync()");

      // Calculate the volumes and convert the grid bounding rectangle into WGS 84 lat/long to return to the caller.
      return await ConvertBoundaryFromGridToWGS84(arg.ProjectID, await request.ExecuteAsync(arg));
    }
  }
}

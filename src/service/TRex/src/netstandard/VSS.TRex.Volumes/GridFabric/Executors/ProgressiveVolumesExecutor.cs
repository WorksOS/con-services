using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Executors
{
  public class ProgressiveVolumesExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProgressiveVolumesExecutor>();

    private async Task<ProgressiveVolumesResponse> ConvertBoundaryFromGridToWGS84(Guid projectUid, ProgressiveVolumesResponse response)
    {
      var convertCoordinates = DIContext.Obtain<IConvertCoordinates>();

      if (response.Volumes != null)
      {
        foreach (var aggregator in response.Volumes)
        {
          if (!aggregator.Volume.BoundingExtentGrid.IsValidPlanExtent) // No conversion possible
            continue;

          var neeCoords = new[] {new XYZ(aggregator.Volume.BoundingExtentGrid.MinX, aggregator.Volume.BoundingExtentGrid.MinY), new XYZ(aggregator.Volume.BoundingExtentGrid.MaxX, aggregator.Volume.BoundingExtentGrid.MaxY)};

          var (errorCode, llhCoords) = await convertCoordinates.NEEToLLH(DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid).CSIB(), neeCoords);

          if (errorCode == RequestErrorStatus.OK)
          {
            aggregator.Volume.BoundingExtentLLH = new BoundingWorldExtent3D {MinX = MathUtilities.RadiansToDegrees(llhCoords[0].X), MinY = MathUtilities.RadiansToDegrees(llhCoords[0].Y), MaxX = MathUtilities.RadiansToDegrees(llhCoords[1].X), MaxY = MathUtilities.RadiansToDegrees(llhCoords[1].Y)};
          }
          else
          {
            Log.LogInformation("Progressive volume failure, could not convert bounding area from grid to WGS coordinates");
            response.ResultStatus = errorCode;
          }
        }
      }

      return response;
    }

    public async Task<ProgressiveVolumesResponse> ExecuteAsync(ProgressiveVolumesRequestArgument arg)
    {
      var request = new ProgressiveVolumesRequest_ClusterCompute();

      Log.LogInformation("Executing ProgressiveVolumesRequestComputeFunc_ApplicationService.ExecuteAsync()");

      // Calculate the volumes and convert the grid bounding rectangle into WGS 84 lat/long to return to the caller.
      return await ConvertBoundaryFromGridToWGS84(arg.ProjectID, await request.ExecuteAsync(arg));
    }
  }
}

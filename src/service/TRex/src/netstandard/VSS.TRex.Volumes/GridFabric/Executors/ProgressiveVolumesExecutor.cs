using System;
using System.Threading.Tasks;
using CoreX.Interfaces;
using CoreX.Types;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Executors
{
  public class ProgressiveVolumesExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProgressiveVolumesExecutor>();

    private ProgressiveVolumesResponse ConvertBoundaryFromGridToWGS84(Guid projectUid, ProgressiveVolumesResponse response)
    {
      var convertCoordinates = DIContext.Obtain<IConvertCoordinates>();

      if (response.Volumes != null)
      {
        foreach (var aggregator in response.Volumes)
        {
          if (!aggregator.Volume.BoundingExtentGrid.IsValidPlanExtent) // No conversion possible
            continue;

          var neeCoords = new[]
          {
            new XYZ(
              aggregator.Volume.BoundingExtentGrid.MinX, aggregator.Volume.BoundingExtentGrid.MinY,
              aggregator.Volume.BoundingExtentGrid.MinZ == Consts.NullDouble ? 0.0 : aggregator.Volume.BoundingExtentGrid.MinZ),
            new XYZ(
              aggregator.Volume.BoundingExtentGrid.MaxX, aggregator.Volume.BoundingExtentGrid.MaxY,
              aggregator.Volume.BoundingExtentGrid.MaxZ == Consts.NullDouble ? 0.0 : aggregator.Volume.BoundingExtentGrid.MaxZ)
          };

          var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid);
          var llhCoords = convertCoordinates.NEEToLLH(siteModel.CSIB(), neeCoords.ToCoreX_XYZ()).ToTRex_XYZ();

          aggregator.Volume.BoundingExtentLLH = new BoundingWorldExtent3D { MinX = MathUtilities.RadiansToDegrees(llhCoords[0].X), MinY = MathUtilities.RadiansToDegrees(llhCoords[0].Y), MaxX = MathUtilities.RadiansToDegrees(llhCoords[1].X), MaxY = MathUtilities.RadiansToDegrees(llhCoords[1].Y) };
        }
      }

      return response;
    }

    public async Task<ProgressiveVolumesResponse> ExecuteAsync(ProgressiveVolumesRequestArgument arg)
    {
      var request = new ProgressiveVolumesRequest_ClusterCompute();

      _log.LogInformation("Executing ProgressiveVolumesRequestComputeFunc_ApplicationService.ExecuteAsync()");

      // Calculate the volumes and convert the grid bounding rectangle into WGS 84 lat/long to return to the caller.
      return ConvertBoundaryFromGridToWGS84(arg.ProjectID, await request.ExecuteAsync(arg));
    }
  }
}

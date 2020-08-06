using System.IO;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Utilities;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// V2 Tile executor. Same as V1 but without the reconcileTopFilterAndVolumeComputationMode as this is done externally.
  /// </summary>
  public class CompactionTileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionTileExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the request for type of T.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<TileRequest>(item);

        if (request.ComputeVolumesType == VolumesType.Between2Filters && !request.ExplicitFilters)
        {
          FilterUtilities.AdjustFilterToFilter(request.Filter1, request.Filter2);
        }

        var trexRequest = new TRexTileRequest(
            request.ProjectUid.Value,
            request.Mode,
            request.Palettes,
            request.DesignDescriptor,
            request.Filter1,
            request.Filter2,
            request.BoundBoxLatLon,
            request.BoundBoxGrid,
            request.Width,
            request.Height,
            AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
            AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings)
          );
          var fileResult = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(trexRequest, "/tile", customHeaders);

          using (var ms = new MemoryStream())
          {
            fileResult.CopyTo(ms);
            return new TileResult(ms.ToArray());
          }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
    }
  }
}

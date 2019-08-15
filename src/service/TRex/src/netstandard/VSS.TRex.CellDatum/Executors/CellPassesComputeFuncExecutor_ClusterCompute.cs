using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Common.Models;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.CellDatum.Executors
{
  public class CellPassesComputeFuncExecutor_ClusterCompute
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellPassesComputeFuncExecutor_ClusterCompute>();

    /// <summary>
    /// Constructor
    /// </summary>
    public CellPassesComputeFuncExecutor_ClusterCompute() {}

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the cell datum
    /// </summary>
    public async Task<CellPassesResponse> ExecuteAsync(CellPassesRequestArgument_ClusterCompute arg, SubGridSpatialAffinityKey key)
    {
      Log.LogInformation($"Performing Execute for DataModel:{arg.ProjectID}");

      var result = new CellPassesResponse { ReturnCode = CellPassesReturnCode.Error };

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
      if (siteModel == null)
      {
        Log.LogError($"Failed to locate site model {arg.ProjectID}");
        return result;
      }

      var existenceMap = siteModel.ExistenceMap;
      var utilities = DIContext.Obtain<IRequestorUtilities>();
      var requestors = utilities.ConstructRequestors(siteModel, arg.Overrides, arg.LiftParams,
        utilities.ConstructRequestorIntermediaries(siteModel, arg.Filters, true, GridDataType.CellPasses),
        AreaControlSet.CreateAreaControlSet(), 
        existenceMap);

      // Get the sub grid relative cell location
      var cellX = arg.OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
      var cellY = arg.OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;

      // Reach into the sub-grid request layer and retrieve an appropriate sub-grid
      var cellOverrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
      cellOverrideMask.SetBit(cellX, cellY);
      requestors[0].CellOverrideMask = cellOverrideMask;

      var thisSubGridOrigin = new SubGridCellAddress(arg.OTGCellX, arg.OTGCellY);
      var requestSubGridInternalResult = await requestors[0].RequestSubGridInternal(thisSubGridOrigin,  true, true);
      if (requestSubGridInternalResult.requestResult != ServerRequestResult.NoError)
      {
        if (requestSubGridInternalResult.requestResult == ServerRequestResult.SubGridNotFound)
          result.ReturnCode = CellPassesReturnCode.NoDataFound;
        else
          Log.LogError($"Request for sub grid {thisSubGridOrigin} request failed with code {requestSubGridInternalResult.requestResult}");
        return result;
      }

      if (!(requestSubGridInternalResult.clientGrid is ClientCellProfileAllPassesLeafSubgrid grid))
      {
        Log.LogError($"Request for sub grid {thisSubGridOrigin} request failed due the grid return type being incorrect. Expected {typeof(ClientCellProfileAllPassesLeafSubgrid).Name}, but got {requestSubGridInternalResult.clientGrid.GetType().Name}");
        return result;
      }

      var cell = grid.Cells[cellX, cellY];
      if (cell.TotalPasses > 0)
      {
        result.ReturnCode = CellPassesReturnCode.DataFound;
        for (var idx = 0; idx < cell.TotalPasses; idx++)
        {
          var cellPass = cell.CellPasses[idx];
          result.CellPasses.Add(cellPass);
        }
      }
      else
        result.ReturnCode = CellPassesReturnCode.NoDataFound;

      return result;
    }
  }
}

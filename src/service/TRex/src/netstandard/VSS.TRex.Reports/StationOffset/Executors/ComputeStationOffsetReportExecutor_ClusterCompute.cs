using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.StationOffset.Executors
{
  /// <summary>
  /// Executes business logic that calculates the profile between two points in space
  /// </summary>
  public class ComputeStationOffsetReportExecutor_ClusterCompute
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeStationOffsetReportExecutor_ClusterCompute>();

    private readonly StationOffsetReportRequestArgument_ClusterCompute requestArgument;

    /// <summary>
    /// Constructs the stationOffset executor
    /// </summary>
    /// <param name="arg"></param>
    public ComputeStationOffsetReportExecutor_ClusterCompute(StationOffsetReportRequestArgument_ClusterCompute arg)
    {
      requestArgument = arg;
    }

    /// <summary>
    /// Executes the stationOffset logic in the cluster compute context where each cluster node processes its fraction of the work and returns the
    /// results to the application service context
    /// </summary>
    public async Task<StationOffsetReportRequestResponse_ClusterCompute> ExecuteAsync()
    {
      StationOffsetReportRequestResponse_ClusterCompute response = null;

      try
        {
          // Note: Start/end point lat/lon fields have been converted into grid local coordinate system by this point
          if (requestArgument.Points.Count > 0)
          {
            Log.LogInformation($"#In#: DataModel {requestArgument.ProjectID}, pointCount: {requestArgument.Points.Count}");
          }
          else
          {
            Log.LogInformation($"#In#: DataModel {requestArgument.ProjectID}, Note! vertices list has insufficient vertices (min of 1 required)");
            return new StationOffsetReportRequestResponse_ClusterCompute{ResultStatus = RequestErrorStatus.OK, ReturnCode = ReportReturnCode.NoData };
          }

          return response = await GetProductionData();
        }
        finally
        {
          Log.LogInformation(
            $"#Out# Execute: DataModel {requestArgument.ProjectID} complete for stationOffset report. #Result#:{response?.ResultStatus ?? RequestErrorStatus.Exception} with {response?.StationOffsetRows.Count ?? 0} offsets");
        }
    }


    /// <summary>
    /// For each point in the list, get the sub grid and extract productionData at the station/offset i.e pointOfInterest
    ///    This could be optimized to get any poi from each sub grid before disposal
    /// </summary>
    private async Task<StationOffsetReportRequestResponse_ClusterCompute> GetProductionData()
    {
      var result = new StationOffsetReportRequestResponse_ClusterCompute {ResultStatus = RequestErrorStatus.Unknown};

      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(requestArgument.ProjectID);
      if (siteModel == null)
      {
        Log.LogError($"Failed to locate site model {requestArgument.ProjectID}");
        return new StationOffsetReportRequestResponse_ClusterCompute {ResultStatus = RequestErrorStatus.NoSuchDataModel};
      }

      IDesign cutFillDesign = null;
      if (requestArgument.ReferenceDesign != null && requestArgument.ReferenceDesign.DesignID != Guid.Empty)
      {
        cutFillDesign = siteModel.Designs.Locate(requestArgument.ReferenceDesign.DesignID);
        if (cutFillDesign == null)
        {
          throw new ArgumentException($"Design {requestArgument.ReferenceDesign.DesignID} not a recognized design in project {requestArgument.ProjectID}");
        }
      }

      var existenceMap = siteModel.ExistenceMap;
      var utilities = DIContext.Obtain<IRequestorUtilities>();
      var requestors = utilities.ConstructRequestors(siteModel,
        utilities.ConstructRequestorIntermediaries(siteModel, requestArgument.Filters, true, GridDataType.CellProfile),
        AreaControlSet.CreateAreaControlSet(), existenceMap);

      // Obtain the primary partition map to allow this request to determine the elements it needs to process
      bool[] primaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions();
      SubGridTreeBitmapSubGridBits cellOverrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
      foreach (var point in requestArgument.Points)
      {
        // Determine the on-the-ground cell 
        siteModel.Grid.CalculateIndexOfCellContainingPosition(point.Easting, point.Northing, out int OTGCellX, out int OTGCellY);

        var thisSubGridOrigin = new SubGridCellAddress(OTGCellX, OTGCellY);

        if (!primaryPartitionMap[thisSubGridOrigin.ToSpatialPartitionDescriptor()])
          continue;

        // Get the sub grid relative cell location
        int cellX = OTGCellX & SubGridTreeConsts.SubGridLocalKeyMask;
        int cellY = OTGCellY & SubGridTreeConsts.SubGridLocalKeyMask;

        // Reach into the sub-grid request layer and retrieve an appropriate sub-grid
        cellOverrideMask.Clear();
        cellOverrideMask.SetBit(cellX, cellY);
        requestors[0].CellOverrideMask = cellOverrideMask;

        // using the cell address get the index of cell in clientGrid
        var requestSubGridInternalResult = await requestors[0].RequestSubGridInternal(thisSubGridOrigin, requestArgument.Overrides, true, true);

        if (requestSubGridInternalResult.requestResult != ServerRequestResult.NoError)
        {
          Log.LogError($"Request for sub grid {thisSubGridOrigin} request failed with code {result}");
          result.StationOffsetRows.Add(new StationOffsetRow(point.Station, point.Offset, point.Northing, point.Easting));
          continue;
        }

        var hydratedPoint = await ExtractRequiredValues(cutFillDesign, point, requestSubGridInternalResult.clientGrid as ClientCellProfileLeafSubgrid, cellX, cellY);
        result.StationOffsetRows.Add(hydratedPoint);
      }

      result.ResultStatus = RequestErrorStatus.OK;
      return result;
    }


    private async Task<StationOffsetRow> ExtractRequiredValues(IDesign cutFillDesign, StationOffsetPoint point, ClientCellProfileLeafSubgrid clientGrid, int cellX, int cellY)
    {
      clientGrid.CalculateWorldOrigin(out double subgridWorldOriginX, out double subgridWorldOriginY);
      var cell = clientGrid.Cells[cellX, cellY];

      var result = new StationOffsetRow(point.Station, point.Offset, cell.CellYOffset + subgridWorldOriginY, cell.CellXOffset + subgridWorldOriginX);
      
      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode) getDesignHeightsResult = (null, DesignProfilerRequestResult.UnknownError);

      if (requestArgument.ReferenceDesign != null && requestArgument.ReferenceDesign.DesignID != Guid.Empty)
      {
        getDesignHeightsResult = await cutFillDesign.GetDesignHeights(requestArgument.ProjectID, requestArgument.ReferenceDesign.Offset, clientGrid.OriginAsCellAddress(), clientGrid.CellSize);

        if (getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK || getDesignHeightsResult.designHeights == null)
        {
          string errorMessage;
          if (getDesignHeightsResult.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
          {
            errorMessage = "StationOffset Report. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
            Log.LogInformation(errorMessage);
          }
          else
          {
            errorMessage = $"StationOffset Report. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {getDesignHeightsResult.errorCode}.";
            Log.LogWarning(errorMessage);
          }
        }
      }

      if (cell.PassCount == 0) // Nothing for us to do, as cell is not in our areaControlSet...
        return result;

      result.Elevation = requestArgument.ReportElevation ? cell.Height : Consts.NullHeight;
      result.CutFill = (requestArgument.ReportCutFill && (getDesignHeightsResult.designHeights != null) &&
                        getDesignHeightsResult.designHeights.Cells[cellX, cellY] != Consts.NullHeight)
        ? cell.Height - getDesignHeightsResult.designHeights.Cells[cellX, cellY]
        : Consts.NullHeight;

      // CCV is equiv to CMV in this instance
      result.Cmv = (short) (requestArgument.ReportCmv ? cell.LastPassValidCCV : CellPassConsts.NullCCV);
      result.Mdp = (short) (requestArgument.ReportMdp ? cell.LastPassValidMDP : CellPassConsts.NullMDP);
      result.PassCount = (short) (requestArgument.ReportPassCount ? cell.PassCount : CellPassConsts.NullPassCountValue);
      result.Temperature = (short) (requestArgument.ReportTemperature ? cell.LastPassValidTemperature : CellPassConsts.NullMaterialTemperatureValue);

      return result;
    }
  }
}

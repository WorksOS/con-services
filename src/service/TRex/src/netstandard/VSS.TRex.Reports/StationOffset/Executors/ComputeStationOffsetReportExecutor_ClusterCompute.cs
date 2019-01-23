using System;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
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

    private StationOffsetReportRequestArgument_ClusterCompute requestArgument;
    //private readonly Guid ProjectID;
    //private readonly GridDataType ProfileTypeRequired;
    //private readonly XYZ[] NEECoords;
    //private readonly IFilterSet Filters;
    //private readonly ProfileStyle ProfileStyle;
    //private readonly VolumeComputationType VolumeType;	

    //private const int INITIAL_PROFILE_LIST_SIZE = 1000;

    //// todo LiftBuildSettings: TICLiftBuildSettings;
    //// ExternalRequestDescriptor: TASNodeRequestDescriptor;

    //private readonly Guid DesignUid;
    //private bool ReturnAllPassesAndLayers;

    //private ISubGridSegmentCellPassIterator CellPassIterator;
    //private ISubGridSegmentIterator SegmentIterator;

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
    public StationOffsetReportRequestResponse Execute()
    {
      StationOffsetReportRequestResponse response = null;
      try
      {
        // var ProfileCells = new List<T>(INITIAL_PROFILE_LIST_SIZE);

        try
        {
          // Note: Start/end point lat/lon fields have been converted into grid local coordinate system by this point
          if (requestArgument.Points.Count > 1)
            Log.LogInformation($"#In#: DataModel {requestArgument.ProjectID}, pointCount: {requestArgument.Points.Count}");
          else
            Log.LogInformation($"#In#: DataModel {requestArgument.ProjectID}, Note! vertices list has insufficient vertices (min of 2 required)");

          ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(requestArgument.ProjectID);

          if (siteModel == null)
          {
            Log.LogWarning($"Failed to locate site model {requestArgument.ProjectID}");
            return response = new StationOffsetReportRequestResponse {ResultStatus = RequestErrorStatus.NoSuchDataModel};
          }

          // Obtain the sub grid existence map for the project
          ISubGridTreeBitMask existenceMap = siteModel.ExistenceMap;

          if (existenceMap == null)
          {
            Log.LogWarning($"Failed to locate production data existence map from sitemodel {requestArgument.ProjectID}");
            return response = new StationOffsetReportRequestResponse {ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap};
          }

          ICellSpatialFilter CellFilter = requestArgument.Filters.Filters[0].SpatialFilter;
          ICellPassAttributeFilter PassFilter = requestArgument.Filters.Filters[0].AttributeFilter;

          // todoJeannie for each requestArg.Point, get subgrid and extract values from productionData into response
          return response = GetProductionData();
        }
        finally
        {
          Log.LogInformation(
            $"#Out# Execute: DataModel {requestArgument.ProjectID} complete for stationOffset report. #Result#:{response?.ResultStatus ?? RequestErrorStatus.Exception} with {response?.StationOffsetReportDataRowList?.Count ?? 0} points");
        }
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception:");
      }

      return new StationOffsetReportRequestResponse();
    }


    /// <summary>
    /// For each point in the list, get the subgrid and extract productionData at the station/offset i.e pointOfInterest
    ///    This could be optimized to get any poi from each subgrid before disposal
    /// </summary>
    private StationOffsetReportRequestResponse GetProductionData()
    {
      foreach (var poi in requestArgument.Points)
      {
        //ClientGrid.Clear;

        //// get cell address
        //SiteModel.Grid.CalculateIndexOfCellContainingPosition(Args.NEECoords[I].X, Args.NEECoords[I].Y, CellAddress.X, CellAddress.Y);

        //CellAddress.ProdDataRequested : = FProdDataExistenceMap.Map[CellAddress.X SHR kSubGridIndexBitsPerLevel, CellAddress.Y SHR kSubGridIndexBitsPerLevel];
        //CellAddress.SurveyedSurfaceDataRequested : = True;

        //// using the celladdress get the index of cell in clientgrid
        //ClientGrid.GetOTGLeafSubGridCellIndex(CellAddress.X, CellAddress.Y, SubGridX, SubGridY);

        //CellOverrideMask.Clear;
        //CellOverrideMask.SetBit(SubGridX, SubGridY); // using cell index turn on bit for cell we are interested in

        //LockTokenName := Format('OffsetProfiling %d', [GetCurrentThreadID]);
        //LockToken := LockTokenManager.FindToken(LockTokenName);
        //if LockToken = -1 then
        //LockToken := LockTokenManager.AcquireToken(LockTokenName)
        //else
        //begin
        //ServerResult := icsrrFailedToLock;
        //SIGLogMessage.PublishNoODS(Self, Format('Lock token name (%s) used for TPSNodeServiceRPCVerb_RequestOffset_Execute instance matches an existing lock token', [LockTokenName]), slmcError);
        //Exit;
        //end;

        //CellProfile := TOffsetProfileCell.Create;
        //CellProfile.Sequence : = trunc(Args.NEECoords[I].Z); // its only a integer anyway

        //try

        //// this is where we get the data
        //DummyControlSet.Init(0, 0, 0, 0, 0, True);

        //if Assigned(ClientGrid) then
        //ServerResult := TSubGridRequestor.RequestSubGridInternal(Nil,
        //PassFilter,
        //VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary,
        //CellFilter,
        //False,
        //T2DBoundingIntegerExtent.Create(0, 0, 0, 0),
        //SiteModel,
        //CellAddress.X, CellAddress.Y,
        //SiteModel.Grid.NumLevels,
        //Args.LiftBuildSettings,
        //LockToken,
        //CellAddress.ProdDataRequested,
        //CellAddress.SurveyedSurfaceDataRequested,
        //ClientGrid,
        //CellOverrideMask,
        //DummyControlSet)
        //else
        //SIGLogMessage.PublishNoODS(Self, Format('TPSNodeServiceRPCVerb_RequestOffset_Execute: Failed to create a client subgrid of type %d', [RequestGridDataType]), slmcError);

        //finally
        //LockTokenManager.ReleaseToken(LockTokenName);
        //end;

        //if ServerResult = icsrrNoError then
        //  begin

        //ExtractRequiredValues(stationOffsetReportRequestArgument, subGrid);
        //CellProfile.Elevation : = TICClientSubGridTreeLeaf_CellProfile(ClientGrid).Cells[SubGridX, SubGridY].Height;
        //CellProfile.CMV : = TICClientSubGridTreeLeaf_CellProfile(ClientGrid).Cells[SubGridX, SubGridY].LastPassValidCCV;
        //CellProfile.MDP : = TICClientSubGridTreeLeaf_CellProfile(ClientGrid).Cells[SubGridX, SubGridY].LastPassValidMDP;
        //CellProfile.PassCount : = TICClientSubGridTreeLeaf_CellProfile(ClientGrid).Cells[SubGridX, SubGridY].PassCount;
        //CellProfile.Temperature : = TICClientSubGridTreeLeaf_CellProfile(ClientGrid).Cells[SubGridX, SubGridY].LastPassValidTemperature;

        //CellProfile.CutFill : = kICNullHeight;
        //if Args.CutFillReport and(CellProfile.Elevation<> kICNullHeight)
        //then
        //  begin
        //    HaveDesignElevationDataForThisSubgrid := Assigned(DesignSubgridExistanceMap) and
        //    DesignSubgridExistanceMap.Cells[CellAddress.X SHR kSubGridIndexBitsPerLevel,
        //      CellAddress.Y SHR kSubGridIndexBitsPerLevel];
        //    if HaveDesignElevationDataForThisSubgrid then
        //      with DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService do
        //        begin
        //          DesignResult := RequestDesignElevationSpot(Construct_CalculateDesignElevationSpot_Args(SiteModel.ID,
        //              Args.NEECoords[I].X,
        //              Args.NEECoords[I].Y,
        //              SiteModel.Grid.CellSize,
        //              Args.DesignDescriptor),
        //            DesignElevation);
        //          if (DesignResult = dppiOK) and
        //          DesignElevation.Success then
        //          if DesignElevation.Height<> kICNullHeight then
        //          CellProfile.CutFill : = CellProfile.Elevation - DesignElevation.Height;
        //        end;
        //  end;
        //end;

        //Packager.CellList.Add(CellProfile);
        //end;
      }
      return new StationOffsetReportRequestResponse { ResultStatus = RequestErrorStatus.Unknown };
    }


    //    //private List<StationOffsetReportDataRow> ExtractRequiredValues(StationOffsetReportRequestArgument stationOffsetReportRequestArgument, ClientCellProfileLeafSubgrid subGrid)
    //    //{
    //    //  var result = new List<StationOffsetReportDataRow>();
    //    //  IClientHeightLeafSubGrid designHeights = null;

    //    //  if (_stationOffsetReportRequestArgument.ReferenceDesignUID != Guid.Empty)
    //    //  {
    //    //    IDesign cutFillDesign = DIContext.Obtain<ISiteModels>().GetSiteModel(_stationOffsetReportRequestArgument.ProjectID).Designs.Locate(_stationOffsetReportRequestArgument.ReferenceDesignUID);
    //    //    if (cutFillDesign == null)
    //    //    {
    //    //      throw new ArgumentException($"Design {_stationOffsetReportRequestArgument.ReferenceDesignUID} not a recognised design in project {_stationOffsetReportRequestArgument.ProjectID}");
    //    //    }

    //    //    cutFillDesign.GetDesignHeights(_stationOffsetReportRequestArgument.ProjectID, subGrid.OriginAsCellAddress(),
    //    //      subGrid.CellSize, out designHeights, out var errorCode);

    //    //    if (errorCode != DesignProfilerRequestResult.OK || designHeights == null)
    //    //    {
    //    //      string errorMessage;
    //    //      if (errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
    //    //      {
    //    //        errorMessage = "StationOffset Report. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
    //    //        Log.LogInformation(errorMessage);
    //    //      }
    //    //      else
    //    //      {
    //    //        errorMessage = $"StationOffset Report. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {errorCode}.";
    //    //        Log.LogWarning(errorMessage);
    //    //      }
    //    //    }
    //    //  }

    //    //  subGrid.CalculateWorldOrigin(out double subgridWorldOriginX, out double subgridWorldOriginY);
    //    //  SubGridUtilities.SubGridDimensionalIterator((x, y) =>
    //    //  {
    //    //    var cell = subGrid.Cells[x, y];

    //    //    if (cell.PassCount == 0) // Nothing for us to do, as cell is not in our areaControlSet...
    //    //      return;

    //    //    // todoJeannie
    //    //    //result.Add(new StationOffsetReportDataRow
    //    //    //{
    //    //    //  Easting = cell.CellXOffset + subgridWorldOriginX,
    //    //    //  Northing = cell.CellYOffset + subgridWorldOriginY,
    //    //    //  Elevation = stationOffsetReportRequestArgument.ReportElevation ? cell.Height : Consts.NullHeight,
    //    //    //  CutFill = (stationOffsetReportRequestArgument.ReportCutFill && (designHeights != null) &&
    //    //    //             designHeights.Cells[x, y] != Consts.NullHeight)
    //    //    //    ? cell.Height - designHeights.Cells[x, y]
    //    //    //    : Consts.NullHeight,

    //    //    //  // CCV is equiv to CMV in this instance
    //    //    //  Cmv = (short) (stationOffsetReportRequestArgument.ReportCmv ? cell.LastPassValidCCV : CellPassConsts.NullCCV),
    //    //    //  Mdp = (short) (stationOffsetReportRequestArgument.ReportMdp ? cell.LastPassValidMDP : CellPassConsts.NullMDP),
    //    //    //  PassCount = (short) (stationOffsetReportRequestArgument.ReportPassCount ? cell.PassCount : CellPassConsts.NullPassCountValue),
    //    //    //  Temperature = (short) (stationOffsetReportRequestArgument.ReportTemperature ? cell.LastPassValidTemperature : CellPassConsts.NullMaterialTemperatureValue)
    //    //    //});
    //    //  });

    //    //  return result;
    //    //}
    //  }
    //}


  }
}

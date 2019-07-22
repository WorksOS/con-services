using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.CellDatum.Executors
{
  public class CellPassesComputeFuncExecutor_ApplicationService
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellPassesComputeFuncExecutor_ApplicationService>();

    /// <summary>
    /// Constructor
    /// </summary>
    public CellPassesComputeFuncExecutor_ApplicationService() {}

    public async Task<CellPassesResponse> ExecuteAsync(CellPassesRequestArgument_ApplicationService arg)
    {
      var result = new CellPassesResponse() {ReturnCode = CellPassesReturnCode.Error};

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
      if (siteModel == null)
      {
        Log.LogError($"Failed to locate site model {arg.ProjectID}");
        return result;
      }

      if (!arg.CoordsAreGrid)
      {
        //WGS84 coords need to be converted to NEE
        arg.Point = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(siteModel.CSIB(), arg.Point);
        result.Northing = arg.Point.Y;
        result.Easting = arg.Point.X;
      }

      var existenceMap = siteModel.ExistenceMap;

      // Determine the on-the-ground cell 
      siteModel.Grid.CalculateIndexOfCellContainingPosition(arg.Point.X, 
        arg.Point.Y, 
        out var otgCellX, 
        out var otgCellY);

      if (!existenceMap[otgCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, otgCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel])
      {
        result.ReturnCode = CellPassesReturnCode.NoDataFound;
        return result;
      }

      var computeArg = new CellPassesRequestArgument_ClusterCompute(arg.ProjectID, arg.Point, otgCellX, otgCellY, arg.Filters);
      var requestCompute = new CellPassesRequest_ClusterCompute();
      var affinityKey = new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, arg.ProjectID, otgCellX, otgCellY);
      var responseCompute = await requestCompute.ExecuteAsync(computeArg, affinityKey);

      result.ReturnCode = responseCompute.ReturnCode;
      result.CellPasses = responseCompute.CellPasses;

      return result;
    }

  }
}

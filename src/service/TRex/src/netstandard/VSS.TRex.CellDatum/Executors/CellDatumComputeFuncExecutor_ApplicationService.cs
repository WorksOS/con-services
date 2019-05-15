using Microsoft.Extensions.Logging;
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
  public class CellDatumComputeFuncExecutor_ApplicationService
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellDatumComputeFuncExecutor_ApplicationService>();

    /// <summary>
    /// Constructor
    /// </summary>
    public CellDatumComputeFuncExecutor_ApplicationService() {}

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the cell datum
    /// </summary>
    public CellDatumResponse_ApplicationService Execute(CellDatumRequestArgument_ApplicationService arg)
    {
      Log.LogInformation($"Performing Execute for DataModel:{arg.ProjectID}, Mode={arg.Mode}");

      var result = new CellDatumResponse_ApplicationService
      { ReturnCode = CellDatumReturnCode.UnexpectedError, DisplayMode = arg.Mode, Northing = arg.Point.Y, Easting = arg.Point.X };
   
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
      if (siteModel == null)
      {
        Log.LogError($"Failed to locate site model {arg.ProjectID}");
        return result;
      }

      if (!arg.CoordsAreGrid)
      {
        //WGS84 coords need to be converted to NEE
        arg.Point = ConvertCoordinates.LLHToNEE(siteModel.CSIB(), arg.Point);
        result.Northing = arg.Point.Y;
        result.Easting = arg.Point.X;
      }

      var existenceMap = siteModel.ExistenceMap;

      // Determine the on-the-ground cell 
      siteModel.Grid.CalculateIndexOfCellContainingPosition(arg.Point.X, arg.Point.Y, out uint OTGCellX, out uint OTGCellY);

      if (!existenceMap[OTGCellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, OTGCellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel])
      {
        result.ReturnCode = CellDatumReturnCode.NoValueFound;
        return result;
      }

      //Now get the production data for this cell
      var argClusterCompute = new CellDatumRequestArgument_ClusterCompute(
        arg.ProjectID, arg.Mode, arg.Point, OTGCellX, OTGCellY, arg.Filters, arg.ReferenceDesign);
      var request = new CellDatumRequest_ClusterCompute();
      var response = request.Execute(argClusterCompute, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER, arg.ProjectID, OTGCellX, OTGCellY));
      result.ReturnCode = response.ReturnCode;
      result.Value = response.Value;
      result.TimeStampUTC = response.TimeStampUTC;

      return result;
    }
  }
}

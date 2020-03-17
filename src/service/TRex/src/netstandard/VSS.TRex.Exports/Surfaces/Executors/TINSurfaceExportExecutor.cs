using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Models;
using VSS.TRex.Common;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.GridDecimator;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Surfaces.Executors
{
  /// <summary>
  /// Generates a decimated TIN surface
  /// </summary>
  public class TINSurfaceExportExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public TINSurfaceRequestResponse SurfaceSubGridsResponse { get; } = new TINSurfaceRequestResponse();

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; }

    private readonly Guid _dataModelId;
    private readonly IFilterSet _filters;
    private readonly double _tolerance;
    private readonly ILiftParameters _liftParams;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public TINSurfaceExportExecutor(
      Guid dataModelId,
      IFilterSet filters,
      double tolerance,
      string requestingTRexNodeId,
      ILiftParameters liftParams
    )
    {
      _dataModelId = dataModelId;
      _filters = filters;
      _tolerance = tolerance;
      RequestingTRexNodeID = requestingTRexNodeId;
      _liftParams = liftParams;
    }

    /// <summary>
    /// Computes a tight bounding extent around the elevation values stored in the sub grid tree
    /// </summary>
    /// <param name="dataStore"></param>
    /// <returns></returns>
    private static BoundingWorldExtent3D DataStoreExtents(ISubGridTree dataStore)
    {
      var computedGridExtent = BoundingWorldExtent3D.Inverted();

      dataStore.ScanAllSubGrids(subGrid =>
      {
        var items = ((GenericLeafSubGrid<float>)subGrid).Items;
        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var elev = items[x, y];
          if (elev != Common.Consts.NullHeight)
            computedGridExtent.Include(subGrid.OriginX + x, subGrid.OriginY + y, elev);
        });

        return true;
      });

      if (computedGridExtent.IsValidPlanExtent)
        computedGridExtent.Offset(-SubGridTreeConsts.DefaultIndexOriginOffset, -SubGridTreeConsts.DefaultIndexOriginOffset);

      // Convert the grid rectangle to a world rectangle, padding out the 3D bound by a small margin to avoid edge effects in calculations
      var computedWorldExtent = new BoundingWorldExtent3D
       ((computedGridExtent.MinX - 1.01) * dataStore.CellSize,
        (computedGridExtent.MinY - 1.01) * dataStore.CellSize,
        (computedGridExtent.MaxX + 1.01) * dataStore.CellSize,
        (computedGridExtent.MaxY + 1.01) * dataStore.CellSize,
        computedGridExtent.MinZ - 0.01, computedGridExtent.MaxZ + 0.01);

      return computedWorldExtent;
    }

    /// <summary>
    /// Executor that implements creation of the TIN surface
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_dataModelId}");

      try
      {
        var requestDescriptor = Guid.NewGuid();
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(_dataModelId);

        if (siteModel == null)
        {
          Log.LogError($"Failed to obtain site model for {_dataModelId}");
          return false;
        }

        var datastore = new GenericSubGridTree<float, GenericLeafSubGrid<float>>(siteModel.Grid.NumLevels, siteModel.CellSize);

        // Provide the processor with a customised request analyser configured to return a set of sub grids. These sub grids
        // are the feed stock for the generated TIN surface
        using (var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
          requestDescriptor,
          _dataModelId,
          GridDataFromModeConverter.Convert(DisplayMode.Height),
          SurfaceSubGridsResponse,
          _filters,
          new DesignOffset(),
          DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.SurfaceExport),
          DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          DIContext.Obtain<IRequestAnalyser>(),
          Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_filters),
          false, //Rendering.Utilities.RequestRequiresAccessToDesignFileExistenceMap(DisplayMode.Height),
          BoundingIntegerExtent2D.Inverted(),
          _liftParams))
        {
          // Set the surface TRexTask parameters for progressive processing
          processor.Task.TRexNodeID = RequestingTRexNodeID;

          if (!await processor.BuildAsync())
          {
            Log.LogError($"Failed to build pipeline processor for request to model {_dataModelId}");
            return false;
          }

          processor.Process();

          if (SurfaceSubGridsResponse.ResultStatus != RequestErrorStatus.OK)
          {
            Log.LogError($"Sub grids response status not OK: {SurfaceSubGridsResponse.ResultStatus}");
            return false;
          }

          // Create the TIN decimator and populate it with the retrieved sub grids
          foreach (var subGrid in ((SurfaceTask) processor.Task).SurfaceSubgrids)
          {
            if (!(datastore.ConstructPathToCell(subGrid.OriginX, subGrid.OriginY, SubGridPathConstructionType.CreatePathToLeaf) is INodeSubGrid newGridNode))
            {
              Log.LogError($"Result from data store ConstructPathToCell({subGrid.OriginX}, {subGrid.OriginY}) was null. Aborting...");
              return false;
            }

            subGrid.Owner = datastore;
            newGridNode.GetSubGridCellIndex(subGrid.OriginX, subGrid.OriginY, out var subGridIndexX, out var subGridIndexY);
            newGridNode.SetSubGrid(subGridIndexX, subGridIndexY, subGrid);
          }
        }

        // Obtain the surface export data smoother and apply it to the tree of queried data before passing it to the decimation engine
        var dataSmoother = DIContext.Obtain<Func<IDataSmoother>>()() as ITreeDataSmoother<float>;
        datastore = dataSmoother?.Smooth(datastore) ?? datastore;

        var extents = DataStoreExtents(datastore);

        // Make sure we don't export too large an area due to data way outside project extents
        if (extents.Area > Common.Consts.MaxExportAreaM2)
        {
          // First try and use project boundary extents as our data boundary
          var canExport = siteModel.SiteModelExtent.Area > 0 && siteModel.SiteModelExtent.Area < Common.Consts.MaxExportAreaM2;
          if (canExport)
          {
            // still use min max height extents
            Log.LogInformation($"Invalid Plan Extent. Data area too large {extents.Area}. Switching to project extents");
            extents.MinX = siteModel.SiteModelExtent.MinX;
            extents.MinY = siteModel.SiteModelExtent.MinY;
            extents.MaxX = siteModel.SiteModelExtent.MaxX;
            extents.MaxY = siteModel.SiteModelExtent.MaxY;
          }
          else
          {
            Log.LogError($"Invalid Plan Extent. Data area too large {extents.Area}.");
            return false;
          }
        }

        // Decimate the elevations into a grid
        var decimator = new GridToTINDecimator(datastore)
        {
          Tolerance = _tolerance
        };
        decimator.SetDecimationExtents(extents);

        if (!decimator.BuildMesh())
        {
          Log.LogError($"Decimator returned false with error code: {decimator.BuildMeshFaultCode}");
          return false;
        }

        // A decimated TIN has been successfully constructed...  Return it!
        SurfaceSubGridsResponse.TIN = decimator.GetTIN();
      }
      catch (Exception e)
      {
        Log.LogError(e, "ExecutePipeline raised Exception:");
        return false;
      }

      return true;
    }
  }
}

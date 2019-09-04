using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Models;
using VSS.TRex.Common;
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
using VSS.TRex.SubGridTrees.Core;
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
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private IFilterSet Filters;
    private double Tolerance;
    private ILiftParameters LiftParams;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
   // private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public TINSurfaceExportExecutor(
      Guid dataModelID,
      IFilterSet filters,
      double tolerance,
      string requestingTRexNodeId,
      ILiftParameters liftParams
    )
    {
      DataModelID = dataModelID;
      Filters = filters;
      Tolerance = tolerance;
      RequestingTRexNodeID = requestingTRexNodeId;
      LiftParams = liftParams;
    }

    /// <summary>
    /// Computes a tight bounding extent around the elevation values stored in the sub grid tree
    /// </summary>
    /// <param name="dataStore"></param>
    /// <returns></returns>
    private BoundingWorldExtent3D DataStoreExtents(GenericSubGridTree_Float dataStore)
    {
      BoundingWorldExtent3D ComputedGridExtent = BoundingWorldExtent3D.Inverted();

      dataStore.ScanAllSubGrids(subGrid =>
      {
        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var elev = ((GenericLeafSubGrid_Float)subGrid).Items[x, y];
          if (elev != Common.Consts.NullHeight)
            ComputedGridExtent.Include((int)(subGrid.OriginX + x), (int)(subGrid.OriginY + y), elev);
        });

        return true;
      });

      if (ComputedGridExtent.IsValidPlanExtent)
        ComputedGridExtent.Offset(-(int)SubGridTreeConsts.DefaultIndexOriginOffset, -(int)SubGridTreeConsts.DefaultIndexOriginOffset);

      // Convert the grid rectangle to a world rectangle, padding out the 3D bound by a small margin to avoid edge effects in calculations
      BoundingWorldExtent3D ComputedWorldExtent = new BoundingWorldExtent3D
       ((ComputedGridExtent.MinX - 1.01) * dataStore.CellSize,
        (ComputedGridExtent.MinY - 1.01) * dataStore.CellSize,
        (ComputedGridExtent.MaxX + 1.01) * dataStore.CellSize,
        (ComputedGridExtent.MaxY + 1.01) * dataStore.CellSize,
        ComputedGridExtent.MinZ - 0.01, ComputedGridExtent.MaxZ + 0.01);

      return ComputedWorldExtent;
    }

    /// <summary>
    /// Executor that implements creation of the TIN surface
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}");

      try
      {
        Guid RequestDescriptor = Guid.NewGuid();
        GenericSubGridTree_Float datastore;

        var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);

        if (SiteModel == null)
        {
          Log.LogError($"Failed to obtain site model for {DataModelID}");
          return false;
        }

        // Provide the processor with a customised request analyser configured to return a set of sub grids. These sub grids
        // are the feed stock for the generated TIN surface
        using (var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
          RequestDescriptor,
          DataModelID,
          GridDataFromModeConverter.Convert(DisplayMode.Height),
          SurfaceSubGridsResponse,
          Filters,
          new DesignOffset(),
          DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.SurfaceExport),
          DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          DIContext.Obtain<IRequestAnalyser>(),
          Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          false, //Rendering.Utilities.RequestRequiresAccessToDesignFileExistenceMap(DisplayMode.Height),
          BoundingIntegerExtent2D.Inverted(),
          LiftParams))
        {
          // Set the surface TRexTask parameters for progressive processing
          processor.Task.RequestDescriptor = RequestDescriptor;
          processor.Task.TRexNodeID = RequestingTRexNodeID;
          processor.Task.GridDataType = GridDataFromModeConverter.Convert(DisplayMode.Height);

          if (!await processor.BuildAsync())
          {
            Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
            return false;
          }

          processor.Process();

          if (SurfaceSubGridsResponse.ResultStatus != RequestErrorStatus.OK)
          {
            Log.LogError($"Sub grids response status not OK: {SurfaceSubGridsResponse.ResultStatus}");
            return false;
          }

          // Create the TIN decimator and populate it with the retrieve sub grids
          datastore = new GenericSubGridTree_Float(SiteModel.Grid.NumLevels, SiteModel.CellSize);
          foreach (var subGrid in ((SurfaceTask) processor.Task).SurfaceSubgrids)
          {
            var newGridNode = datastore.ConstructPathToCell(subGrid.OriginX, subGrid.OriginY, SubGridPathConstructionType.CreatePathToLeaf) as INodeSubGrid;

            if (newGridNode == null)
            {
              Log.LogError($"Result from data store ConstructPathToCell({subGrid.OriginX}, {subGrid.OriginY}) was null. Aborting...");
              return false;
            }

            subGrid.Owner = datastore;
            newGridNode.GetSubGridCellIndex(subGrid.OriginX, subGrid.OriginY, out byte subGridIndexX, out byte subGridIndexY);
            newGridNode.SetSubGrid(subGridIndexX, subGridIndexY, subGrid);
          }
        }

        var extents = DataStoreExtents(datastore);
        // Make sure we don't export too large an area due to data way outside project extents
        if (extents.Area > Consts.MaxExportAreaM2)
        {
          // First try and use project boundary extents as our data boundary
          var canExport = SiteModel.SiteModelExtent.Area > 0 && SiteModel.SiteModelExtent.Area < Consts.MaxExportAreaM2;
          if (canExport)
          {
            // still use min max height extents
            Log.LogInformation($"Invalid Plan Extent. Data area too large {extents.Area}. Switching to project extents");
            extents.MinX = SiteModel.SiteModelExtent.MinX;
            extents.MinY = SiteModel.SiteModelExtent.MinY;
            extents.MaxX = SiteModel.SiteModelExtent.MaxX;
            extents.MaxY = SiteModel.SiteModelExtent.MaxY;
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
          Tolerance = Tolerance
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
      catch (Exception E)
      {
        Log.LogError(E, "ExecutePipeline raised Exception:");
        return false;
      }

      return true;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.RequestStatistics;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class GriddedReportExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public GriddedReportRequestResponse GriddedReportRequestResponse { get; set; } = new GriddedReportRequestResponse();

    private readonly GriddedReportRequestArgument _griddedReportRequestArgument;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public GriddedReportExecutor(GriddedReportRequestArgument arg) => _griddedReportRequestArgument = arg;

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_griddedReportRequestArgument.ProjectID}");

      try
      {
        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid requestDescriptor = Guid.NewGuid();

        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: requestDescriptor,
          dataModelID: _griddedReportRequestArgument.ProjectID,
          siteModel: null,
          gridDataType: GridDataType.CellProfile,
          response: GriddedReportRequestResponse,
          filters: _griddedReportRequestArgument.Filters,
          cutFillDesignID: _griddedReportRequestArgument.ReferenceDesignID,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_griddedReportRequestArgument.Filters),
          requestRequiresAccessToDesignFileExistenceMap: _griddedReportRequestArgument.ReferenceDesignID != Guid.Empty,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

        // Set the surface TRexTask parameters for progressive processing
        processor.Task.RequestDescriptor = requestDescriptor;
        processor.Task.TRexNodeID = _griddedReportRequestArgument.TRexNodeID;
        processor.Task.GridDataType = GridDataType.CellProfile;
        // todoJeannie how does this differ to the one in Task: processor.GridDataType
        

        //todoJeannie what to do with: GridReportOption (endpoint, direction); GridInterval; Azimuth

        // todoJeannie which extents? also any Unit conversion?
        //processor.RequestAnalyser.WorldExtents or this?
        processor.OverrideSpatialExtents = new BoundingWorldExtent3D(_griddedReportRequestArgument.StartEasting,
          _griddedReportRequestArgument.StartNorthing,
          _griddedReportRequestArgument.EndEasting,
          _griddedReportRequestArgument.EndNorthing);

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {_griddedReportRequestArgument.ProjectID}");
          return false;
        }
        
        processor.Process();

        if (GriddedReportRequestResponse.ResultStatus == RequestErrorStatus.OK)
        {
          foreach (var subGrid in ((GriddedReportTask)processor.Task).ResultantSubgrids)
          {
            if (!(subGrid is ClientCellProfileLeafSubgrid))
              continue;

            GriddedReportRequestResponse.GriddedReportDataRowList
              .AddRange(ExtractRequiredValues(_griddedReportRequestArgument, (ClientCellProfileLeafSubgrid)subGrid));
          }
        }
      }
      catch (Exception E)
      {
        Log.LogError($"ExecutePipeline raised exception {E}");
        return false;
      }

      return true;
    }

    private List<GriddedReportDataRow> ExtractRequiredValues(GriddedReportRequestArgument griddedReportRequestArgument, ClientCellProfileLeafSubgrid subGrid)
    {
      var result = new List<GriddedReportDataRow>();
      foreach (var cell in subGrid.Cells)
      {
        result.Add(new GriddedReportDataRow
        {
          Easting = cell.CellXOffset + subGrid.CacheOriginX, // todoJeannie what unit to convert to?
          Northing = cell.CellYOffset + subGrid.CacheOriginY, // todoJeannie what unit to convert to?
          Elevation = griddedReportRequestArgument.ReportElevation ? cell.Height : 0.0, // todoJeannie what is the default?
          // todoJeannie CutFill = (griddedReportRequestArgument.ReportCutFill ? cell.? : 0)
          Cmv = (short)(griddedReportRequestArgument.ReportCMV ? cell.LastPassValidCCV : 0),
          Mdp = (short)(griddedReportRequestArgument.ReportMDP ? cell.LastPassValidMDP : 0),
          PassCount = (short)(griddedReportRequestArgument.ReportPassCount ? cell.PassCount : 0),
          Temperature = (short)(griddedReportRequestArgument.ReportTemperature ? cell.LastPassValidTemperature : 0)
        });
      }
      return result;
    }
  }
}

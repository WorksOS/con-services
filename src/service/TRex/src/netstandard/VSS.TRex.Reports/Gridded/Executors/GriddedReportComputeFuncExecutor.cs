using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class GriddedReportComputeFuncExecutor
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
    public GriddedReportComputeFuncExecutor(GriddedReportRequestArgument arg) => _griddedReportRequestArgument = arg;

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
          cutFillDesignID: _griddedReportRequestArgument.ReferenceDesignUID,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_griddedReportRequestArgument.Filters),
          requestRequiresAccessToDesignFileExistenceMap: _griddedReportRequestArgument.ReferenceDesignUID != Guid.Empty,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

        // Set the grid TRexTask parameters for progressive processing
        processor.Task.RequestDescriptor = requestDescriptor;
        processor.Task.TRexNodeID = _griddedReportRequestArgument.TRexNodeID;
        processor.Task.GridDataType = GridDataType.CellProfile;

        ((GriddedReportTask)processor.Task).ProcessorDelegate = 
          subGrid => GriddedReportRequestResponse.GriddedReportDataRowList
            .AddRange(ExtractRequiredValues(_griddedReportRequestArgument, (ClientCellProfileLeafSubgrid)subGrid));


        // report options 0=direction,1=endpoint,2=automatic
        if (_griddedReportRequestArgument.GridReportOption == GridReportOption.EndPoint)
        {
          // Compute the bearing between the two points as a survey (north azimuth, clockwise increasing)
          _griddedReportRequestArgument.Azimuth = Math.Atan2(_griddedReportRequestArgument.EndNorthing - _griddedReportRequestArgument.StartNorthing, _griddedReportRequestArgument.EndEasting - _griddedReportRequestArgument.StartEasting);
        }
        else
        {
          if (_griddedReportRequestArgument.GridReportOption == GridReportOption.Automatic)
          {
            // automatic
            _griddedReportRequestArgument.Azimuth = 0;
            _griddedReportRequestArgument.StartNorthing = 0;
            _griddedReportRequestArgument.StartEasting = 0;
          }
        }

        // Interval will be >= 0.1m and <= 100.0m
        processor.Pipeline.AreaControlSet =
          new AreaControlSet(false, _griddedReportRequestArgument.GridInterval, _griddedReportRequestArgument.GridInterval,
            _griddedReportRequestArgument.StartEasting, _griddedReportRequestArgument.StartNorthing,
            _griddedReportRequestArgument.Azimuth);

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {_griddedReportRequestArgument.ProjectID}");
          return false;
        }
        
        processor.Process();

        if (GriddedReportRequestResponse.ResultStatus != RequestErrorStatus.OK)
        {
          throw new ArgumentException($"Unable to obtain data for Gridded report. GriddedReportRequestResponse: {GriddedReportRequestResponse.ResultStatus.ToString()}.");
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "ExecutePipeline raised exception");
        return false;
      }

      return true;
    }

    private List<GriddedReportDataRow> ExtractRequiredValues(GriddedReportRequestArgument griddedReportRequestArgument, ClientCellProfileLeafSubgrid subGrid)
    {
      var result = new List<GriddedReportDataRow>();
      IClientHeightLeafSubGrid designHeights = null;

      if (_griddedReportRequestArgument.ReferenceDesignUID != Guid.Empty)
      {
        IDesign cutFillDesign = DIContext.Obtain<ISiteModels>().GetSiteModel(_griddedReportRequestArgument.ProjectID).Designs.Locate(_griddedReportRequestArgument.ReferenceDesignUID);
        if (cutFillDesign == null)
        {
          throw new ArgumentException($"Design {_griddedReportRequestArgument.ReferenceDesignUID} not a recognised design in project {_griddedReportRequestArgument.ProjectID}");
        }

        cutFillDesign.GetDesignHeights(_griddedReportRequestArgument.ProjectID, subGrid.OriginAsCellAddress(),
          subGrid.CellSize, out designHeights, out var errorCode);

        if (errorCode != DesignProfilerRequestResult.OK || designHeights == null)
        {
          string errorMessage;
          if (errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
          {
            errorMessage = "Gridded Report. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
            Log.LogInformation(errorMessage);
          }
          else
          {
            errorMessage = $"Gridded Report. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {designHeights}.";
            Log.LogWarning(errorMessage);
          }
        }
      }

      subGrid.CalculateWorldOrigin(out double subgridWorldOriginX, out double subgridWorldOriginY);
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = subGrid.Cells[x, y];

        if (cell.PassCount == 0) // Nothing for us to do, as cell is not in our areaControlSet...
          return;

        result.Add(new GriddedReportDataRow
        {
          Easting = cell.CellXOffset + subgridWorldOriginX,
          Northing = cell.CellYOffset + subgridWorldOriginY,
          Elevation = griddedReportRequestArgument.ReportElevation ? cell.Height : Consts.NullHeight,
          CutFill = (griddedReportRequestArgument.ReportCutFill && (designHeights != null) &&
                     designHeights.Cells[x, y] != Consts.NullHeight)
            ? cell.Height - designHeights.Cells[x, y]
            : Consts.NullHeight,

          // CCV is equiv to CMV in this instance
          Cmv = (short) (griddedReportRequestArgument.ReportCMV ? cell.LastPassValidCCV : CellPassConsts.NullCCV),
          Mdp = (short) (griddedReportRequestArgument.ReportMDP ? cell.LastPassValidMDP : CellPassConsts.NullMDP),
          PassCount = (short) (griddedReportRequestArgument.ReportPassCount ? cell.PassCount : CellPassConsts.NullPassCountValue),
          Temperature = (short) (griddedReportRequestArgument.ReportTemperature ? cell.LastPassValidTemperature : CellPassConsts.NullMaterialTemperatureValue)
        });
      });

      return result;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class GriddedReportComputeFuncExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);
    private const Double StartGridOffset = 0.000001; // by offsetting the grid start position a tiny distance we avoid skipped cells due to cell boundary checks

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public GriddedReportRequestResponse GriddedReportRequestResponse { get; } = new GriddedReportRequestResponse();

    private readonly GriddedReportRequestArgument _griddedReportRequestArgument;
    private readonly ISiteModel _siteModel;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public GriddedReportComputeFuncExecutor(GriddedReportRequestArgument arg)
    {
      _griddedReportRequestArgument = arg;
      if (arg != null)
        _siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
    }

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_griddedReportRequestArgument.ProjectID}");

      ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

      var requestDescriptor = Guid.NewGuid();

      var task = DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport) as GriddedReportTask;

      processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
        requestDescriptor,
        _griddedReportRequestArgument.ProjectID,
        GridDataType.CellProfile,
        GriddedReportRequestResponse,
        _griddedReportRequestArgument.Filters,
        _griddedReportRequestArgument.ReferenceDesign,
        task,
        DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        DIContext.Obtain<IRequestAnalyser>(),
        Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_griddedReportRequestArgument.Filters),
        _griddedReportRequestArgument.ReferenceDesign != null && _griddedReportRequestArgument.ReferenceDesign.DesignID != Guid.Empty,
        BoundingIntegerExtent2D.Inverted(),
        _griddedReportRequestArgument.LiftParams
      );

      // Set the grid TRexTask parameters for progressive processing
      processor.Task.RequestDescriptor = requestDescriptor;
      processor.Task.TRexNodeID = _griddedReportRequestArgument.TRexNodeID;
      processor.Task.GridDataType = GridDataType.CellProfile;

      task.ProcessorDelegate = async subGrid => GriddedReportRequestResponse.GriddedReportDataRowList
          .AddRange(await ExtractRequiredValues(_griddedReportRequestArgument, subGrid));

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

      // Avoid starting on cell boundary by applying tiny offset
      _griddedReportRequestArgument.StartNorthing = StartGridOffset;
      _griddedReportRequestArgument.StartEasting  = StartGridOffset;

      // Interval will be >= 0.1m and <= 100.0m
      processor.Pipeline.AreaControlSet =
        new AreaControlSet(false, _griddedReportRequestArgument.GridInterval, _griddedReportRequestArgument.GridInterval,
          _griddedReportRequestArgument.StartEasting, _griddedReportRequestArgument.StartNorthing,
          _griddedReportRequestArgument.Azimuth);

      if (!await processor.BuildAsync())
      {
        Log.LogError($"Failed to build pipeline processor for request to model {_griddedReportRequestArgument.ProjectID}");
        return false;
      }

      processor.Process();

      if (GriddedReportRequestResponse.ResultStatus != RequestErrorStatus.OK)
      {
        throw new ArgumentException($"Unable to obtain data for Gridded report. GriddedReportRequestResponse: {GriddedReportRequestResponse.ResultStatus.ToString()}.");
      }

      return true;
    }

    private async Task<List<GriddedReportDataRow>> ExtractRequiredValues(GriddedReportRequestArgument griddedReportRequestArgument, ClientCellProfileLeafSubgrid subGrid)
    {
      var result = new List<GriddedReportDataRow>();
      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult errorCode) getDesignHeightsResult = (null, DesignProfilerRequestResult.UnknownError);

      if (_griddedReportRequestArgument.ReferenceDesign != null && _griddedReportRequestArgument.ReferenceDesign.DesignID != Guid.Empty)
      {
        var cutFillDesign = _siteModel.Designs.Locate(_griddedReportRequestArgument.ReferenceDesign.DesignID);
        if (cutFillDesign == null)
          throw new ArgumentException($"Design {_griddedReportRequestArgument.ReferenceDesign.DesignID} not a recognized design in project {_griddedReportRequestArgument.ProjectID}");

        getDesignHeightsResult = await cutFillDesign.GetDesignHeights(_griddedReportRequestArgument.ProjectID, _griddedReportRequestArgument.ReferenceDesign.Offset, 
          subGrid.OriginAsCellAddress(), subGrid.CellSize);

        if (getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK || getDesignHeightsResult.designHeights == null)
        {
          string errorMessage;
          if (getDesignHeightsResult.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
          {
            errorMessage = "Gridded Report. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
            Log.LogInformation(errorMessage);
          }
          else
          {
            errorMessage = $"Gridded Report. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {getDesignHeightsResult.errorCode}.";
            Log.LogWarning(errorMessage);
          }
        }
      }

      subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = subGrid.Cells[x, y];

        if (cell.PassCount == 0) // Nothing for us to do, as cell is not in our areaControlSet...
          return;

        result.Add(new GriddedReportDataRow
        {
          Easting = cell.CellXOffset + subGridWorldOriginX,
          Northing = cell.CellYOffset + subGridWorldOriginY,
          Elevation = griddedReportRequestArgument.ReportElevation ? cell.Height : Consts.NullHeight,
          CutFill = griddedReportRequestArgument.ReportCutFill && getDesignHeightsResult.designHeights != null && getDesignHeightsResult.designHeights.Cells[x, y] != Consts.NullHeight
            ? cell.Height - getDesignHeightsResult.designHeights.Cells[x, y]
            : Consts.NullHeight,

          // CCV is equiv to CMV in this instance
          Cmv = griddedReportRequestArgument.ReportCmv ? cell.LastPassValidCCV : CellPassConsts.NullCCV,
          Mdp = griddedReportRequestArgument.ReportMdp ? cell.LastPassValidMDP : CellPassConsts.NullMDP,
          PassCount = (short) (griddedReportRequestArgument.ReportPassCount ? cell.PassCount : CellPassConsts.NullPassCountValue),
          Temperature = (short) (griddedReportRequestArgument.ReportTemperature ? cell.LastPassValidTemperature : CellPassConsts.NullMaterialTemperatureValue)
        });
      });

      return result;
    }
  }
}

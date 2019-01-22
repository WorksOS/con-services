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
using VSS.TRex.Reports.StationOffset.Executors.Tasks;
using VSS.TRex.Reports.StationOffset.GridFabric;
using VSS.TRex.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.StationOffset.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class StationOffsetReportComputeFuncExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public StationOffsetReportRequestResponse StationOffsetReportRequestResponse { get; set; } = new StationOffsetReportRequestResponse();

    private readonly StationOffsetReportRequestArgument _stationOffsetReportRequestArgument;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public StationOffsetReportComputeFuncExecutor(StationOffsetReportRequestArgument arg) => _stationOffsetReportRequestArgument = arg;

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the StationOffset rows
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_stationOffsetReportRequestArgument.ProjectID}");

      try
      {
        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid requestDescriptor = Guid.NewGuid();

        // todoJeannie
        //processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: requestDescriptor,
        //  dataModelID: _stationOffsetReportRequestArgument.ProjectID,
        //  siteModel: null,
        //  gridDataType: GridDataType.CellProfile,
        //  response: StationOffsetReportRequestResponse,
        //  filters: _stationOffsetReportRequestArgument.Filters,
        //  cutFillDesignID: _stationOffsetReportRequestArgument.ReferenceDesignUID,
        //  task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport),
        //  pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        //  requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
        //  requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_stationOffsetReportRequestArgument.Filters),
        //  requestRequiresAccessToDesignFileExistenceMap: _stationOffsetReportRequestArgument.ReferenceDesignUID != Guid.Empty,
        //  overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
        //  );

        //// Set the grid TRexTask parameters for progressive processing
        //processor.Task.RequestDescriptor = requestDescriptor;
        //processor.Task.TRexNodeID = _stationOffsetReportRequestArgument.TRexNodeID;
        //processor.Task.GridDataType = GridDataType.CellProfile;

        //((StationOffsetReportTask)processor.Task).ProcessorDelegate = 
        //  subGrid => StationOffsetReportRequestResponse.StationOffsetReportDataRowList
        //    .AddRange(ExtractRequiredValues(_stationOffsetReportRequestArgument, (ClientCellProfileLeafSubgrid)subGrid));


        //// report options 0=direction,1=endpoint,2=automatic
        //if (_stationOffsetReportRequestArgument.GridReportOption == GridReportOption.EndPoint)
        //{
        //  // Compute the bearing between the two points as a survey (north azimuth, clockwise increasing)
        //  _stationOffsetReportRequestArgument.Azimuth = Math.Atan2(_stationOffsetReportRequestArgument.EndNorthing - _stationOffsetReportRequestArgument.StartNorthing, _stationOffsetReportRequestArgument.EndEasting - _stationOffsetReportRequestArgument.StartEasting);
        //}
        //else
        //{
        //  if (_stationOffsetReportRequestArgument.GridReportOption == GridReportOption.Automatic)
        //  {
        //    // automatic
        //    _stationOffsetReportRequestArgument.Azimuth = 0;
        //    _stationOffsetReportRequestArgument.StartNorthing = 0;
        //    _stationOffsetReportRequestArgument.StartEasting = 0;
        //  }
        //}

        //// Interval will be >= 0.1m and <= 100.0m
        //processor.Pipeline.AreaControlSet =
        //  new AreaControlSet(false, _stationOffsetReportRequestArgument.GridInterval, _stationOffsetReportRequestArgument.GridInterval,
        //    _stationOffsetReportRequestArgument.StartEasting, _stationOffsetReportRequestArgument.StartNorthing,
        //    _stationOffsetReportRequestArgument.Azimuth);

        //if (!processor.Build())
        //{
        //  Log.LogError($"Failed to build pipeline processor for request to model {_stationOffsetReportRequestArgument.ProjectID}");
        //  return false;
        //}
        
        //processor.Process();

        if (StationOffsetReportRequestResponse.ResultStatus != RequestErrorStatus.OK)
        {
          throw new ArgumentException($"Unable to obtain data for StationOffset report. StationOffsetReportRequestResponse: {StationOffsetReportRequestResponse.ResultStatus.ToString()}.");
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "ExecutePipeline raised exception");
        return false;
      }

      return true;
    }

    private List<StationOffsetReportDataRow> ExtractRequiredValues(StationOffsetReportRequestArgument stationOffsetReportRequestArgument, ClientCellProfileLeafSubgrid subGrid)
    {
      var result = new List<StationOffsetReportDataRow>();
      IClientHeightLeafSubGrid designHeights = null;

      if (_stationOffsetReportRequestArgument.ReferenceDesignUID != Guid.Empty)
      {
        IDesign cutFillDesign = DIContext.Obtain<ISiteModels>().GetSiteModel(_stationOffsetReportRequestArgument.ProjectID).Designs.Locate(_stationOffsetReportRequestArgument.ReferenceDesignUID);
        if (cutFillDesign == null)
        {
          throw new ArgumentException($"Design {_stationOffsetReportRequestArgument.ReferenceDesignUID} not a recognised design in project {_stationOffsetReportRequestArgument.ProjectID}");
        }

        cutFillDesign.GetDesignHeights(_stationOffsetReportRequestArgument.ProjectID, subGrid.OriginAsCellAddress(),
          subGrid.CellSize, out designHeights, out var errorCode);

        if (errorCode != DesignProfilerRequestResult.OK || designHeights == null)
        {
          string errorMessage;
          if (errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
          {
            errorMessage = "StationOffset Report. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
            Log.LogInformation(errorMessage);
          }
          else
          {
            errorMessage = $"StationOffset Report. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {errorCode}.";
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

        // todoJeannie
        //result.Add(new StationOffsetReportDataRow
        //{
        //  Easting = cell.CellXOffset + subgridWorldOriginX,
        //  Northing = cell.CellYOffset + subgridWorldOriginY,
        //  Elevation = stationOffsetReportRequestArgument.ReportElevation ? cell.Height : Consts.NullHeight,
        //  CutFill = (stationOffsetReportRequestArgument.ReportCutFill && (designHeights != null) &&
        //             designHeights.Cells[x, y] != Consts.NullHeight)
        //    ? cell.Height - designHeights.Cells[x, y]
        //    : Consts.NullHeight,

        //  // CCV is equiv to CMV in this instance
        //  Cmv = (short) (stationOffsetReportRequestArgument.ReportCmv ? cell.LastPassValidCCV : CellPassConsts.NullCCV),
        //  Mdp = (short) (stationOffsetReportRequestArgument.ReportMdp ? cell.LastPassValidMDP : CellPassConsts.NullMDP),
        //  PassCount = (short) (stationOffsetReportRequestArgument.ReportPassCount ? cell.PassCount : CellPassConsts.NullPassCountValue),
        //  Temperature = (short) (stationOffsetReportRequestArgument.ReportTemperature ? cell.LastPassValidTemperature : CellPassConsts.NullMaterialTemperatureValue)
        //});
      });

      return result;
    }
  }
}

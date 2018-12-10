using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Designs.Interfaces;
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
          cutFillDesignID: _griddedReportRequestArgument.ReferenceDesignID,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_griddedReportRequestArgument.Filters),
          requestRequiresAccessToDesignFileExistenceMap: _griddedReportRequestArgument.ReferenceDesignID != Guid.Empty,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

        // Set the grid TRexTask parameters for progressive processing
        processor.Task.RequestDescriptor = requestDescriptor;
        processor.Task.TRexNodeID = _griddedReportRequestArgument.TRexNodeID;
        processor.Task.GridDataType = GridDataType.CellProfile;

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

        processor.Pipeline.AreaControlSet =
          new AreaControlSet(_griddedReportRequestArgument.GridInterval, _griddedReportRequestArgument.GridInterval,
            _griddedReportRequestArgument.StartEasting, _griddedReportRequestArgument.StartNorthing,
            _griddedReportRequestArgument.Azimuth, false);

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
      IDesign CutFillDesign = null;
      IClientHeightLeafSubGrid DesignHeights = null;

      if (_griddedReportRequestArgument.ReferenceDesignID != Guid.Empty)
      {
        CutFillDesign = DIContext.Obtain<ISiteModels>().GetSiteModel(_griddedReportRequestArgument.ProjectID).Designs.Locate(_griddedReportRequestArgument.ReferenceDesignID);
        if (CutFillDesign == null)
          throw new ArgumentException($"Design {_griddedReportRequestArgument.ReferenceDesignID} not a recognised design in project {_griddedReportRequestArgument.ProjectID}");

        if (!CutFillDesign.GetDesignHeights(_griddedReportRequestArgument.ProjectID, subGrid.OriginAsCellAddress(),
          subGrid.CellSize, out DesignHeights, out var errorCode))
          DesignHeights = null;
      }

      var result = new List<GriddedReportDataRow>();
      // use of FIndexOriginOffset?
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
          Elevation = griddedReportRequestArgument.ReportElevation ? cell.Height : Consts.NullHeight, // todoJeannie convert defaults later on to the default for 3dp
          CutFill = (griddedReportRequestArgument.ReportCutFill && (DesignHeights != null) &&
                     DesignHeights.Cells[x, y] != Consts.NullHeight)
            ? cell.Height - DesignHeights.Cells[x, y]
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

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;
using VSS.TRex.Pipelines.Interfaces.Tasks;

namespace VSS.TRex.Volumes
{
  /// <summary>
  /// ProgressiveVolumesCalculator implements an algorithm that computes a time series of volume information useful for
  /// supporting burn up/burn down style progress analysis.
  /// </summary>
  public class ProgressiveVolumesCalculator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProgressiveVolumesCalculator>();

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public ProgressiveVolumesResponse VolumesRequestResponse { get; } = new ProgressiveVolumesResponse();

    /// <summary>
    /// The Aggregator to use for calculating volumes statistics
    /// </summary>
    public ISubGridRequestsAggregator Aggregator { get; set; }

    /// <summary>
    /// The site model from which the volume is being calculated
    /// </summary>
    public ISiteModel SiteModel { get; set; }

    /// <summary>
    /// The volume computation method to use when calculating volume information
    /// </summary>
    public VolumeComputationType VolumeType = VolumeComputationType.None;

    /// <summary>
    /// Parameters for lift analysis
    /// </summary>
    public ILiftParameters LiftParams { get; set; }

    /// <summary>
    ///  Default no-arg constructor
    /// </summary>
    // ReSharper disable once EmptyConstructor
    public ProgressiveVolumesCalculator()
    {
    }

    protected BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

    /// <summary>
    /// The single filter governing spatial, temporal and limit attribute filtering of the cell passes used
    /// to compute a progressive time series of volume calculations.
    /// When performing calculations across a time range the start/end times to be spanned shall be designated
    /// within this filter. FOr 'between to filters' volumes, this filter shall fulfill the role of start and
    /// end filters in that the only component differing between the two filters are the startAt and endAt
    /// elements
    /// </summary>
    public ICombinedFilter Filter { get; set; }

    /// <summary>
    /// RefOriginal references a subset that may be used in the volumes calculations
    /// process. If set, it represents the original ground of the site
    /// </summary>
    public IDesign RefOriginal { get; set; }

    /// <summary>
    /// RefDesign references a subset that may be used in the volumes calculations
    /// process. If set, it takes the place of the 'top' filter.
    /// </summary>
    public IDesign RefDesign { get; set; }

    /// <summary>
    /// ActiveDesign is the design surface being used as the comparison surface in the
    /// surface to production data volume calculations. It is assigned from the FRefOriginal
    /// and FRefDesign surfaces depending on the volumes reporting type and configuration.
    /// It also contains the offset for a reference surface.
    /// </summary>
    public IDesignWrapper ActiveDesign { get; set; }

    /// <summary>
    /// FromSelectionType and ToSelectionType describe how we mix the two filters
    /// (BaseFilter and TopFilter) and two reference designs (RefOriginal and
    /// RefDesign) together to derive the upper and lower 'surfaces' between which
    /// we compute summary or details production volumes
    /// </summary>
    public ProdReportSelectionType FromSelectionType { get; set; } = ProdReportSelectionType.None;

    /// <summary>
    /// FromSelectionType and ToSelectionType describe how we mix the two filters
    /// (BaseFilter and TopFilter) and two reference designs (RefOriginal and
    /// RefDesign) together to derive the upper and lower 'surfaces' between which
    /// we compute summary or details production volumes
    /// </summary>
    public ProdReportSelectionType ToSelectionType { get; set; } = ProdReportSelectionType.None;

    /// <summary>
    /// Determines if the progressive volume calculation should include surveyed surfaces in the results
    /// </summary>
    public bool UseSurveyedSurfaces { get; set; }

    public bool AbortedDueToTimeout { get; set; } = false;

    public Guid RequestDescriptor { get; set; } = Guid.Empty;

    public async Task<bool> ExecutePipeline()
    {
      try
      {
        try
        {
          if (ActiveDesign != null && (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.BetweenDesignAndFilter))
          {
            if (ActiveDesign == null || ActiveDesign.Design.DesignDescriptor.IsNull)
            {
              Log.LogError($"No design provided to prod data/design volumes calc for datamodel {SiteModel.ID}");
              VolumesRequestResponse.ResultStatus = RequestErrorStatus.NoDesignProvided;
              return false;
            }
          }

          using (var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild
          (requestDescriptor: RequestDescriptor,
            dataModelID: SiteModel.ID,
            gridDataType: GridDataType.Height,
            response: VolumesRequestResponse,
            cutFillDesign: null,
            filters: new FilterSet(Filter),
            task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.ProgressiveVolumes),
            pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultAggregative),
            requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
            requestRequiresAccessToDesignFileExistenceMap: RefDesign != null || RefOriginal != null,
            requireSurveyedSurfaceInformation: UseSurveyedSurfaces, //_filteredSurveyedSurfaces.Count > 0,
            overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted(),
            liftParams: LiftParams
          ))
          {
            processor.Task.RequestDescriptor = RequestDescriptor;
            //processor.Task.TRexNodeID = _CSVExportRequestArgument.TRexNodeID;

            if (!await processor.BuildAsync())
            {
              Log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
              VolumesRequestResponse.ResultStatus = processor.Response.ResultStatus;

              return false;
            }

            processor.Process();

            if (processor.Response.ResultStatus != RequestErrorStatus.OK)
            {
              Log.LogError($"Failed to compute progressive volumes data, for project: {SiteModel.ID}. response: {processor.Response.ResultStatus.ToString()}.");
              VolumesRequestResponse.ResultStatus = processor.Response.ResultStatus;
              return false;
            }

            //        if (((CSVExportTask) processor.Task).DataRows.Count > 0)
            //        {
            //
            //        }

            return true;
          }
        }
        catch (Exception e)
        {
          Log.LogError(e, "Pipeline processor raised exception");
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception");
      }

      return false;
    }

    public Task<bool> ComputeVolumeInformation()
    {
      if (VolumeType == VolumeComputationType.None)
        throw new TRexException("No report type supplied to ComputeVolumeInformation");

      if (FromSelectionType == ProdReportSelectionType.Surface)
      {
        if (RefOriginal == null)
        {
          Log.LogError("No RefOriginal surface supplied");
          return Task.FromResult(false);
        }
      }

      if (ToSelectionType == ProdReportSelectionType.Surface)
      {
        if (RefDesign == null)
        {
          Log.LogError("No RefDesign surface supplied");
          return Task.FromResult(false);
        }
      }

      // Compute the volumes as required
      return ExecutePipeline();
    }
  }
}

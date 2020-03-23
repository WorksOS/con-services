using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.Executors.Tasks;
using VSS.TRex.Volumes.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;

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
    /// Injected context for access to ExistenceMaps functionality
    /// </summary>
    private IExistenceMaps _existenceMaps;

    private IExistenceMaps GetExistenceMaps() => _existenceMaps ?? (_existenceMaps = DIContext.Obtain<IExistenceMaps>());

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
    /// BaseFilter and TopFilter reference two sets of filter settings
    /// between which we may calculate volumes. At the current time, it is
    /// meaningful for a filter to have a spatial extent, and to denote an
    /// 'as-at' time only.
    /// </summary>
/*    public ICombinedFilter BaseFilter { get; set; }

    public ICombinedFilter TopFilter { get; set; }
    */

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

    /*
    // FAborted keeps track of whether we've been bushwhacked or not!
    protected FAborted : Boolean;
    */

    /// <summary>
    /// UseEarliestData governs whether we want the earliest or latest data from filtered
    /// ranges of cell passes in the base filtered surface.
    /// </summary>
    public bool UseEarliestData { get; set; }

    private ISubGridTreeBitMask _prodDataExistenceMap;
    private ISubGridTreeBitMask _overallExistenceMap;

    private ISubGridTreeBitMask _designSubGridOverlayMap;

    public bool AbortedDueToTimeout { get; set; } = false;

    readonly ISurveyedSurfaces _filteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

//    readonly ISurveyedSurfaces _filteredBaseSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
//    readonly ISurveyedSurfaces _filteredTopSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

    public Guid RequestDescriptor { get; set; } = Guid.Empty;

    private void ConfigurePipeline(SubGridPipelineAggregative<SubGridsRequestArgument, ProgressiveVolumesResponse> pipeLine)
    {
      pipeLine.RequestDescriptor = RequestDescriptor;
      //PipeLine.ExternalDescriptor := FExternalDescriptor;

      pipeLine.DataModelID = SiteModel.ID;

      Log.LogDebug($"Volume calculation extents for DM={SiteModel.ID}, Request={RequestDescriptor}: {Extents}");

      pipeLine.OverallExistenceMap = _overallExistenceMap;
      pipeLine.ProdDataExistenceMap = _prodDataExistenceMap;
      pipeLine.DesignSubGridOverlayMap = _designSubGridOverlayMap;

      // Initialise a request analyzer to provide to the pipeline
      pipeLine.RequestAnalyser = DIContext.Obtain<IRequestAnalyser>();
      pipeLine.RequestAnalyser.Pipeline = pipeLine;
      pipeLine.RequestAnalyser.WorldExtents.Assign(Extents);

      pipeLine.LiftParams = LiftParams;

      // Construct and assign the filter set into the pipeline
      IFilterSet filterSet = new FilterSet(Filter);

/*    if (VolumeType == VolumeComputationType.Between2Filters)
      {
        filterSet = new FilterSet(BaseFilter, TopFilter);
      }
      else
      {
        filterSet = VolumeType == VolumeComputationType.BetweenDesignAndFilter ? new FilterSet(TopFilter) : new FilterSet(BaseFilter);
      }
      */

      pipeLine.FilterSet = filterSet;
      pipeLine.GridDataType = GridDataType.Height;

      //      if (_filteredTopSurveyedSurfaces.Count > 0 || _filteredBaseSurveyedSurfaces.Count > 0)
      //        pipeLine.IncludeSurveyedSurfaceInformation = true;
      pipeLine.IncludeSurveyedSurfaceInformation = _filteredSurveyedSurfaces.Count > 0;
    }

    public RequestErrorStatus ExecutePipeline()
    {
      SubGridPipelineAggregative<SubGridsRequestArgument, ProgressiveVolumesResponse> pipeLine;

      var result = RequestErrorStatus.Unknown;

      var pipelineAborted = false;
      // bool ShouldAbortDueToCompletedEventSet  = false;

      try
      {
        _prodDataExistenceMap = SiteModel.ExistenceMap;

        try
        {
          if (ActiveDesign != null && (VolumeType == VolumeComputationType.BetweenFilterAndDesign || VolumeType == VolumeComputationType.BetweenDesignAndFilter))
          {
            if (ActiveDesign == null || ActiveDesign.Design.DesignDescriptor.IsNull)
            {
              Log.LogError($"No design provided to prod data/design volumes calc for datamodel {SiteModel.ID}");
              return RequestErrorStatus.NoDesignProvided;
            }

            _designSubGridOverlayMap = GetExistenceMaps().GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, ActiveDesign.Design.ID);

            if (_designSubGridOverlayMap == null)
              return RequestErrorStatus.NoDesignProvided;
          }

          _overallExistenceMap = new SubGridTreeSubGridExistenceBitMask();

          // Work out the surveyed surfaces and coverage areas that need to be taken into account

          var surveyedSurfaces = SiteModel.SurveyedSurfaces;

          if (surveyedSurfaces != null)
          {
            // See if we need to handle surveyed surface data
            // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
            if (!surveyedSurfaces.ProcessSurveyedSurfacesForFilter(SiteModel.ID, Filter, _filteredSurveyedSurfaces, _filteredSurveyedSurfaces, _overallExistenceMap))
              return RequestErrorStatus.Unknown;

            /*
            // See if we need to handle surveyed surface data for 'base'
            // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
            if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenFilterAndDesign)
            {
              if (!surveyedSurfaces.ProcessSurveyedSurfacesForFilter(SiteModel.ID, BaseFilter, _filteredTopSurveyedSurfaces, _filteredBaseSurveyedSurfaces, _overallExistenceMap))
                return RequestErrorStatus.Unknown;
            }

            // See if we need to handle surveyed surface data for 'top'
            // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
            if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
            {
              if (!surveyedSurfaces.ProcessSurveyedSurfacesForFilter(SiteModel.ID, TopFilter, _filteredBaseSurveyedSurfaces, _filteredTopSurveyedSurfaces, _overallExistenceMap))
                return RequestErrorStatus.Unknown;
            }
            */
          }

          // Add in the production data existence map to the computed surveyed surfaces existence maps
          _overallExistenceMap.SetOp_OR(_prodDataExistenceMap);

          // If necessary, impose spatial constraints from filter design(s)
          if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, Filter, _overallExistenceMap))
            return RequestErrorStatus.Unknown;

          /*
          if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenFilterAndDesign)
          {
            if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, BaseFilter, _overallExistenceMap))
              return RequestErrorStatus.Unknown;
          }

          if (VolumeType == VolumeComputationType.Between2Filters || VolumeType == VolumeComputationType.BetweenDesignAndFilter)
          {
            if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, TopFilter, _overallExistenceMap))
              return RequestErrorStatus.Unknown;
          }
          */

          var pipelinedTask = new VolumesComputationTask {Aggregator = Aggregator};

          try
          {
            pipeLine = new SubGridPipelineAggregative<SubGridsRequestArgument, ProgressiveVolumesResponse>( /*0, */ pipelinedTask);
            pipelinedTask.PipeLine = pipeLine;

            ConfigurePipeline(pipeLine);

            if (pipeLine.Initiate())
            {
              pipeLine.WaitForCompletion()
                .ContinueWith(x =>
                {
                  Log.LogInformation(x.Result ? "WaitForCompletion successful" : $"WaitForCompletion timed out with {pipeLine.SubGridsRemainingToProcess} sub grids remaining to be processed");
                })
                .Wait();
            }

            pipelineAborted = pipeLine.Aborted;

            if (!pipeLine.Terminated && !pipeLine.Aborted)
              result = RequestErrorStatus.OK;
          }
          finally
          {
            if (AbortedDueToTimeout)
              result = RequestErrorStatus.AbortedDueToPipelineTimeout;
            else if (pipelinedTask.IsCancelled || pipelineAborted)
              result = RequestErrorStatus.RequestHasBeenCancelled;
          }
        }
        catch (Exception e)
        {
          Log.LogError(e, "ExecutePipeline raised exception");
        }

        return result;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception");
      }

      return RequestErrorStatus.Unknown;
    }

    private void ApplyFilterAndSubsetBoundariesToExtents()
    {
      Filter.SpatialFilter.CalculateIntersectionWithExtents(Extents);

      /*
      if (FromSelectionType == ProdReportSelectionType.Filter)
        BaseFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);

      if (ToSelectionType == ProdReportSelectionType.Filter)
        TopFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
      */
    }

    public bool ComputeVolumeInformation()
    {
      if (VolumeType == VolumeComputationType.None)
        throw new TRexException("No report type supplied to ComputeVolumeInformation");

      if (FromSelectionType == ProdReportSelectionType.Surface)
      {
        if (RefOriginal == null)
        {
          Log.LogError("No RefOriginal surface supplied");
          return false;
        }
      }

      if (ToSelectionType == ProdReportSelectionType.Surface)
      {
        if (RefDesign == null)
        {
          Log.LogError("No RefDesign surface supplied");
          return false;
        }
      }

      // Adjust the extents we have been given to encompass the spatial extent
      // of the supplied filters (if any);
      ApplyFilterAndSubsetBoundariesToExtents();

//      BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass = UseEarliestData;

      // Compute the volume as required
      return ExecutePipeline() == RequestErrorStatus.OK;
    }

    /*
    public RequestErrorStatus ExecutePipelineEx()
    {
      using (var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild
      (requestDescriptor: RequestDescriptor,
        dataModelID: SiteModel.ID,
        siteModel: SiteModel,
        gridDataType: GridDataType.Height,
        response: new ProgressiveVolumesResponse(), 
        filters: new FilterSet(BaseFilter, TopFilter),
        cutFillDesign: ReferenceDesign,
        task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.ProgressiveVolumes),
        pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultAggregative),
        requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
        requestRequiresAccessToDesignFileExistenceMap: ReferenceDesignUID != Guid.Empty,
        requireSurveyedSurfaceInformation: IncludeSurveyedSurfaces,
        overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
      ))
      {
      }

      return RequestErrorStatus.OK;
    }
    */
  }
}

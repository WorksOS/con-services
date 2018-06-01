using System;
using VSS.TRex.Designs;
using VSS.TRex.Executors.Tasks.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Surfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Supports construction and configuration of a subgrid request pipeline that mediates and orchestrates
  /// subgrid based queries
  /// </summary>
  public class PipelineProcessor : IPipelineProcessor
  {
    public Guid RequestDescriptor;

    /// <summary>
    /// The identifier of the project the request is operating on.
    /// </summary>
    public Guid DataModelID;

    /// <summary>
    /// The set of surveyed surfaces that are to be excluded from the computed results for the request
    /// </summary>
    public Guid[] SurveyedSurfaceExclusionList = new Guid[0];

    /// <summary>
    /// The set of filters to be executed for each subgrid examined in the request. Each filters will result in a computed
    /// subgrid variation for subsequent business logic in the pipeline task to operate on.
    /// </summary>
    public FilterSet Filters;

    /// <summary>
    /// The spatial extents derived from the parameters when building the pipeline
    /// </summary>
    public BoundingWorldExtent3D SpatialExtents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

    /// <summary>
    /// The response used as the return from the pipeline request
    /// </summary>
    public SubGridsPipelinedReponseBase Response;

    /// <summary>
    /// The query context mode of data to be processed and/or returned by the query (eg: Height, CutFill etc)
    /// </summary>
    public DisplayMode Mode;

    public SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap;
    public SubGridTreeSubGridExistenceBitMask OverallExistenceMap;
    public SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap;

    /// <summary>
    /// Flag indicating if all surveyed surface have been excluded from the request due to time fitlering constraints
    /// </summary>
    public bool SurveyedSurfacesExludedViaTimeFiltering;

    /// <summary>
    /// The identifier for any cut/fill design refefence being supplied to the request
    /// </summary>
    public Guid CutFillDesignID;

    /// <summary>
    /// Records if the pipeline was aborted before completing operations
    /// </summary>
    public bool PipelineAborted { get; set; }

    /// <summary>
    /// The task to be fitted to the pipelien to mediate subgrid retrieval and procesing
    /// </summary>
    public ITask Task { get; set; }

    /// <summary>
    /// The pipe lien used to retrive subgrids from the cluster compute layer
    /// </summary>
    public ISubGridPipelineBase Pipeline { get; set; }

    /// <summary>
    /// The request analyser used to determine the subgrids to be sent to the cluster compute layer
    /// </summary>
    public IRequestAnalyser RequestAnalyser { get; set; }

    /// <summary>
    /// Indicates if the pipeline was aborted due to a TTL timeout
    /// </summary>
    public bool AbortedDueToTimeout { get; set; }

    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="dataModelID"></param>
    /// <param name="response"></param>
    /// <param name="filters"></param>
    /// <param name="mode"></param>
    /// <param name="cutFillDesignID"></param>
    /// <param name="task"></param>
    /// <param name="pipeline"></param>
    /// <param name="requestAnalyser"></param>
    public PipelineProcessor(Guid requestDescriptor,
                             Guid dataModelID,
                             DisplayMode mode,
                             SubGridsPipelinedReponseBase response,
                             FilterSet filters,
                             Guid cutFillDesignID,
                             ITask task,
                             ISubGridPipelineBase pipeline,
                             IRequestAnalyser requestAnalyser)
    {
      RequestDescriptor = requestDescriptor;
      DataModelID = dataModelID;
      Mode = mode;
      Response = response;
      Filters = filters;
      CutFillDesignID = cutFillDesignID;
      Task = task;
      Pipeline = pipeline;

      // Introduce the task and the pipeline to each other
      Pipeline.PipelineTask = Task;
      Task.PipeLine = Pipeline;

      RequestAnalyser = requestAnalyser;
      // Introduce the Request analyser to the pipeline and spatial extents is requires
      RequestAnalyser.Pipeline = Pipeline;
      RequestAnalyser.WorldExtents = SpatialExtents;
    }

    /// <summary>
    /// Builds the pipeline configured per the supplied state ready to exesute the request
    /// </summary>
    /// <returns></returns>
    public bool Build()
    {
      foreach (var filter in Filters.Filters)
      {
        if (filter != null && SurveyedSurfaceExclusionList.Length > 0)
        {
          SurveyedSurfaceExclusionList = new Guid[filter.AttributeFilter.SurveyedSurfaceExclusionList.Length];
          Array.Copy(filter.AttributeFilter.SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList,
            SurveyedSurfaceExclusionList.Length);
        }
      }

      // Get the SiteModel for the request
      ISiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(DataModelID);
      if (SiteModel == null)
      {
        throw new ArgumentException($"Unable to acquire site model instance for ID:{DataModelID}");
      }

      SpatialExtents = SiteModel.GetAdjustedDataModelSpatialExtents(SurveyedSurfaceExclusionList);

      if (!SpatialExtents.IsValidPlanExtent)
      {
        Response.ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // TODO: Or there was no data in the model
        return false;
      }

      // Get the current production data existance map from the sitemodel
      ProdDataExistenceMap = SiteModel.GetProductionDataExistanceMap(SiteModels.SiteModels.Instance().ImmutableStorageProxy);

      if (ProdDataExistenceMap == null)
      {
        Response.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
        return false;
      }

      // Obtain the subgrid existence map for the project
      // Retrieve the existence map for the datamodel
      OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask()
      {
        CellSize = SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize
      };

      if (Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode))
      {
        // Obtain local reference to surveyed surfaces (lock free access)
        SurveyedSurfaces LocalSurveyedSurfaces = SiteModel.SurveyedSurfaces;

        // Construct two filtered survyed surface lists to act as a rolling pair used as arguments
        // to the ProcessSurveyedSurfacesForFilter method
        SurveyedSurfaces FilterSurveyedSurfaces = new SurveyedSurfaces();
        SurveyedSurfaces FilteredSurveyedSurfaces = new SurveyedSurfaces();

        foreach (var filter in Filters.Filters)
        {
          if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(DataModelID, LocalSurveyedSurfaces, filter,
            FilteredSurveyedSurfaces, FilterSurveyedSurfaces, OverallExistenceMap))
          {
            Response.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
            return false;
          }

          SurveyedSurfacesExludedViaTimeFiltering |= FilterSurveyedSurfaces.Count > 0;
        }
      }

      OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

      foreach (var filter in Filters.Filters)
      {
        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel.ID, filter, OverallExistenceMap))
        {
          Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        if (filter?.AttributeFilter.AnyFilterSelections == true)
        {
          Response.ResultStatus = FilterUtilities.PrepareFilterForUse(filter, DataModelID);
          if (Response.ResultStatus != RequestErrorStatus.OK)
            return false;
        }
      }

      Filters.ApplyFilterAndSubsetBoundariesToExtents(ref SpatialExtents);

      // If this request involves a relationship with a design then ensure the existance map
      // for the design is loaded in to memory to allow the request pipeline to confine
      // subgrid requests that overlay the actual design
      if (Rendering.Utilities.RequestRequiresAccessToDesignFileExistanceMap(Mode /*, ReferenceVolumeType*/))
      {
        /* if (CutFillDesign.IsNull)
        {
            Log.LogError($"No design provided to cut fill, summary volume or thickness overlay render request for datamodel {DataModelID}");
            PatchSubGridsResponse.ResultStatus = RequestErrorStatus.NoDesignProvided;
            return false;
        }*/

        DesignSubgridOverlayMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(DataModelID,
          ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, CutFillDesignID);

        if (DesignSubgridOverlayMap == null)
        {
          //Log.LogError($"Failed to request subgrid overlay index for design {CutFillDesignID} in datamodel {DataModelID}");
          Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        DesignSubgridOverlayMap.CellSize = SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize;
      }

      ConfigurePipeline();

      return true;
    }

    /// <summary>
    /// Configures pipeline specific settings into the pipeline aspect of the processor
    /// </summary>
    protected void ConfigurePipeline()
    {
      Pipeline.RequestDescriptor = RequestDescriptor;

      // PipeLine.ExternalDescriptor  = ExternalDescriptor;
      // PipeLine.SubmissionNode.DescriptorType  = cdtWMSTile;
      // PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;

      Pipeline.DataModelID = DataModelID;

      // todo PipeLine.LiftBuildSettings  = FICOptions.GetLiftBuildSettings(FFilter1.LayerMethod);

      // If summaries of compaction information (both CMV and MDP) are being displayed,
      // and the lift build settings requires all layers to be examined (so the
      // apropriate summarize top layer only flag is false), then instruct the layer
      // analysis engine to apply to restriction to the number of cell passes to use
      // to perform layer analysis (ie: all cell passes will be used).

      if (Mode == DisplayMode.CCVSummary || Mode == DisplayMode.CCVPercentSummary)
      {
        /* TODO... if (!PipeLine.LiftBuildSettings.CCVSummarizeTopLayerOnly)
            PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
        */
      }

      if (Mode == DisplayMode.MDPSummary || Mode == DisplayMode.MDPPercentSummary)
      {
        /* TODO... if (!PipeLine.LiftBuildSettings.MDPSummarizeTopLayerOnly)
            PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
        */
      }

      Pipeline.OverallExistenceMap = OverallExistenceMap;
      Pipeline.ProdDataExistenceMap = ProdDataExistenceMap;
      Pipeline.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

      Pipeline.GridDataType = GridDataFromModeConverter.Convert(Mode);

      // Assign the filter set into the pipeline
      Pipeline.FilterSet = Filters;

      Pipeline.WorldExtents.Assign(SpatialExtents);

      Pipeline.IncludeSurveyedSurfaceInformation = Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) && !SurveyedSurfacesExludedViaTimeFiltering;
      if (Pipeline.IncludeSurveyedSurfaceInformation)  // if required then check if filter turns off requirement due to filters used
        Pipeline.IncludeSurveyedSurfaceInformation = Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters);

      //PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;
    }

    /// <summary>
    /// Performing all processing activities to retrieve subgrids
    /// </summary>
    public void Process()
    {
      try
      {
        if (Pipeline.Initiate())
          Pipeline.WaitForCompletion();

        PipelineAborted = Pipeline.Aborted;

        if (!Pipeline.Terminated && !Pipeline.Aborted)
          Response.ResultStatus = RequestErrorStatus.OK;
      }
      finally
      {
        if (AbortedDueToTimeout)
          Response.ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
        else
        {
          if (Task.IsCancelled || PipelineAborted)
            Response.ResultStatus = RequestErrorStatus.RequestHasBeenCancelled;
        }
      }
    }
  }
}

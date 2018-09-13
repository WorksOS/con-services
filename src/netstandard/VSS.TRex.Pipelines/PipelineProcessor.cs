using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Supports construction and configuration of a subgrid request pipeline that mediates and orchestrates
  /// subgrid based queries
  /// </summary>
  public class PipelineProcessor : IPipelineProcessor
  {
    private static ILogger Log = Logging.Logger.CreateLogger<PipelineProcessor>();

    private IExistenceMaps existenceMaps = null;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ?? (existenceMaps = DIContext.Obtain<IExistenceMaps>());

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
    public IFilterSet Filters;

    /// <summary>
    /// The spatial extents derived from the parameters when building the pipeline
    /// </summary>
    public BoundingWorldExtent3D SpatialExtents { get; set; } = BoundingWorldExtent3D.Full();

    /// <summary>
    /// The response used as the return from the pipeline request
    /// </summary>
    public ISubGridsPipelinedReponseBase Response { get; set; }

    /// <summary>
    /// Grid data type to be processed and/or returned by the query (eg: Height, CutFill etc)
    /// </summary>
    public GridDataType GridDataType { get; set; }

    public ISubGridTreeBitMask ProdDataExistenceMap { get; set; }
    public ISubGridTreeBitMask OverallExistenceMap { get; set; }
    public ISubGridTreeBitMask DesignSubgridOverlayMap { get; set; }

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
    /// Reference to the site model incolved in the request
    /// </summary>
    public ISiteModel SiteModel { get; set; }

    /// <summary>
    /// Indicates if the pipeline was aborted due to a TTL timeout
    /// </summary>
    public bool AbortedDueToTimeout { get; set; }

    /// <summary>
    /// Indicates if the pipeline requests should include surveyed surface information
    /// </summary>
    public bool RequireSurveyedSurfaceInformation { get; set; }

    /// <summary>
    /// If this request involves a relationship with a design then ensure the existance map
    /// for the design is loaded in to memory to allow the request pipeline to confine
    /// subgrid requests that overlay the actual design
    /// </summary>
    public bool RequestRequiresAccessToDesignFileExistanceMap { get; set; }

    /// <summary>
    /// A restriction on the cells that are returned via the query that intersects with the spatial seelction filtering and criteria
    /// </summary>
    public BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; }

    /// <summary>
    /// Constructs the context of a pipelined processor based on the project, filters and other common criteria
    /// of pipelined requests
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="dataModelID"></param>
    /// <param name="gridDataType"></param>
    /// <param name="response"></param>
    /// <param name="filters"></param>
    /// <param name="cutFillDesignID"></param>
    /// <param name="task"></param>
    /// <param name="pipeline"></param>
    /// <param name="requestAnalyser"></param>
    /// <param name="requireSurveyedSurfaceInformation"></param>
    /// <param name="requestRequiresAccessToDesignFileExistanceMap"></param>
    /// <param name="overrideSpatialCellRestriction">A restriction on the cells that are returned via the query that intersects with the spatial seelction filtering and criteria</param>
    /// <param name="siteModel"></param>
    public PipelineProcessor(Guid requestDescriptor,
                             Guid dataModelID,
                             ISiteModel siteModel,
                             GridDataType gridDataType,
                             ISubGridsPipelinedReponseBase response,
                             IFilterSet filters,
                             Guid cutFillDesignID,
                             ITask task,
                             ISubGridPipelineBase pipeline,
                             IRequestAnalyser requestAnalyser,
                             bool requireSurveyedSurfaceInformation,
                             bool requestRequiresAccessToDesignFileExistanceMap,
                             BoundingIntegerExtent2D overrideSpatialCellRestriction)
    {
      RequestDescriptor = requestDescriptor;
      DataModelID = dataModelID;
      SiteModel = siteModel;
      GridDataType = gridDataType;
      Response = response;
      Filters = filters;
      CutFillDesignID = cutFillDesignID;
      Task = task;
      Pipeline = pipeline;

      RequestAnalyser = requestAnalyser;

      RequireSurveyedSurfaceInformation = requireSurveyedSurfaceInformation;
      RequestRequiresAccessToDesignFileExistanceMap = requestRequiresAccessToDesignFileExistanceMap;

      OverrideSpatialCellRestriction = overrideSpatialCellRestriction;
    }

    /// <summary>
    /// Builds the pipeline configured per the supplied state ready to exesute the request
    /// </summary>
    /// <returns></returns>
    public bool Build()
    {
      // Introduce the task and the pipeline to each other
      Pipeline.PipelineTask = Task;
      Task.PipeLine = Pipeline;

      // Introduce the Request analyser to the pipeline and spatial extents it requires
      RequestAnalyser.Pipeline = Pipeline;
      RequestAnalyser.WorldExtents = SpatialExtents;

      // Construct an aggregated set of excluded surveyed surfaces for the filters used in the query
      foreach (var filter in Filters.Filters)
      {
        if (filter != null && SurveyedSurfaceExclusionList.Length > 0)
        {
          SurveyedSurfaceExclusionList = new Guid[filter.AttributeFilter.SurveyedSurfaceExclusionList.Length];
          Array.Copy(filter.AttributeFilter.SurveyedSurfaceExclusionList, SurveyedSurfaceExclusionList,
            SurveyedSurfaceExclusionList.Length);
        }
      }

      if (SiteModel == null)
      {
        // Get the SiteModel for the request
        SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
        if (SiteModel == null)
        {
          Response.ResultStatus = RequestErrorStatus.NoSuchDataModel;
          return false;
        }
      }

      SpatialExtents = SiteModel.GetAdjustedDataModelSpatialExtents(SurveyedSurfaceExclusionList);

      if (!SpatialExtents.IsValidPlanExtent)
      {
        Response.ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // Or there was no data in the model
        return false;
      }

      // Get the current production data existance map from the sitemodel
      ProdDataExistenceMap = SiteModel.GetProductionDataExistanceMap(DIContext.Obtain<ISiteModels>().StorageProxy);
      
      if (ProdDataExistenceMap == null)
      {
        Response.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
        return false;
      }

      // Obtain the subgrid existence map for the project
      // Retrieve the existence map for the datamodel
      OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask
      {
        CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModel.Grid.CellSize
      };

      if (RequireSurveyedSurfaceInformation)
      {
        // Obtain local reference to surveyed surfaces (lock free access)
        ISurveyedSurfaces LocalSurveyedSurfaces = SiteModel.SurveyedSurfaces;

        if (LocalSurveyedSurfaces != null)
        {
          // Construct two filtered surveyed surface lists to act as a rolling pair used as arguments
          // to the ProcessSurveyedSurfacesForFilter method
          ISurveyedSurfaces FilterSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();
          ISurveyedSurfaces FilteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

          foreach (var filter in Filters.Filters)
          {
            if (!LocalSurveyedSurfaces.ProcessSurveyedSurfacesForFilter(DataModelID, filter,
              FilteredSurveyedSurfaces, FilterSurveyedSurfaces, OverallExistenceMap))
            {
              Response.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
              return false;
            }

            SurveyedSurfacesExludedViaTimeFiltering |= FilterSurveyedSurfaces.Count > 0;
          }
        }
      }

      OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

      foreach (var filter in Filters.Filters)
      {
        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel, filter, OverallExistenceMap))
        {
          Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        if (filter?.AttributeFilter.AnyFilterSelections == true)
        {
          Response.ResultStatus = FilterUtilities.PrepareFilterForUse(filter, DataModelID);
          if (Response.ResultStatus != RequestErrorStatus.OK)
          {
            Log.LogInformation($"PrepareFilterForUse failed: Datamodel={DataModelID}");
            return false;
          }
        }
      }

      // Adjust the extents we have been given to encompass the spatial extent of the supplied filters (if any)
      Filters.ApplyFilterAndSubsetBoundariesToExtents(SpatialExtents);

      // If this request involves a relationship with a design then ensure the existance map
      // for the design is loaded in to memory to allow the request pipeline to confine
      // subgrid requests that overlay the actual design
      if (RequestRequiresAccessToDesignFileExistanceMap)
      {
        if (CutFillDesignID == Guid.Empty)
        {
            Log.LogError($"No design provided to cut fill, summary volume or thickness overlay render request for datamodel {DataModelID}");
            Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
            return false;
        }

        DesignSubgridOverlayMap = GetExistenceMaps().GetSingleExistenceMap(DataModelID, ExistenceMaps.Interfaces.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, CutFillDesignID);

        if (DesignSubgridOverlayMap == null)
        {
          Log.LogError($"Failed to request subgrid overlay index for design {CutFillDesignID} in datamodel {DataModelID}");
          Response.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        DesignSubgridOverlayMap.CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModel.Grid.CellSize;
      }

      ConfigurePipeline();

      return true;
    }

    /// <summary>
    /// Configures pipeline specific settings into the pipeline aspect of the processor
    /// </summary>
    protected void ConfigurePipeline()
    {
      Pipeline.GridDataType = GridDataType;

      Pipeline.RequestDescriptor = RequestDescriptor;

      // PipeLine.ExternalDescriptor  = ExternalDescriptor;
      // PipeLine.SubmissionNode.DescriptorType  = cdtWMSTile;
      // PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;

      Pipeline.DataModelID = DataModelID;

      //TODO Readd when lift build settings are supported
      // PipeLine.LiftBuildSettings  = FICOptions.GetLiftBuildSettings(FFilter1.LayerMethod);

      // If summaries of compaction information (both CMV and MDP) are being displayed,
      // and the lift build settings requires all layers to be examined (so the
      // apropriate summarize top layer only flag is false), then instruct the layer
      // analysis engine to apply to restriction to the number of cell passes to use
      // to perform layer analysis (ie: all cell passes will be used).

      /* Todo: Delegate this kind of specialised configuration to the client of the pipeline processor
      if (Mode == DisplayMode.CCVSummary || Mode == DisplayMode.CCVPercentSummary)
      {
        if (!PipeLine.LiftBuildSettings.CCVSummarizeTopLayerOnly)
           PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
      }

      if (Mode == DisplayMode.MDPSummary || Mode == DisplayMode.MDPPercentSummary)
      {
        if (!PipeLine.LiftBuildSettings.MDPSummarizeTopLayerOnly)
          PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
      }
      */

      Pipeline.OverallExistenceMap = OverallExistenceMap;
      Pipeline.ProdDataExistenceMap = ProdDataExistenceMap;
      Pipeline.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

      // Assign the filter set into the pipeline
      Pipeline.FilterSet = Filters;

      Log.LogDebug($"Extents for query against DM={DataModelID}: {SpatialExtents}");

      Pipeline.WorldExtents.Assign(SpatialExtents);

      Pipeline.IncludeSurveyedSurfaceInformation = RequireSurveyedSurfaceInformation && !SurveyedSurfacesExludedViaTimeFiltering;

//      Pipeline.OverrideSpatialCellRestriction = OverrideSpatialCellRestriction;

      //PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;

      Pipeline.RequestAnalyser = RequestAnalyser;
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

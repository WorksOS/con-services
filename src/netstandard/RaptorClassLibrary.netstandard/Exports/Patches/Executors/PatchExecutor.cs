using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Designs;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.RequestStatistics;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Surfaces; 
using VSS.TRex.Types;
using VSS.TRex.Utilities;

namespace VSS.TRex.Rendering.Patches.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class PatchExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object avbailable for inspection once the Executor has completed processing
    /// </summary>
    public PatchRequestResponse PatchSubGridsResponse { get; set; } = new PatchRequestResponse();
    
    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    // FExternalDescriptor :TASNodeRequestDescriptor;

    private DisplayMode Mode;

    private PatchTask PipelinedTask;

    private SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse> PipeLine;
    private SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap;
    private SubGridTreeSubGridExistenceBitMask OverallExistenceMap;
    private SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap;

    private FilterSet Filters;

    private long RequestDescriptor;

    private BoundingWorldExtent3D SpatialExtents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

    public bool AbortedDueToTimeout = false;

    private bool SurveyedSurfacesExludedViaTimeFiltering;

    private int DataPatchPageNumber;
    private int DataPatchPageSize;

    /// <summary>
    /// The identifier for the design held in the designs list ofr the project to be used to calculate cut/fill values
    /// </summary>
    public Guid CutFillDesignID { get; set; }

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="mode"></param>
    /// <param name="filters"></param>
    /// <param name="cutFillDesignID"></param>
    /// <param name="requestingTRexNodeId"></param>
    public PatchExecutor(Guid dataModelID,
      //AExternalDescriptor :TASNodeRequestDescriptor;
      DisplayMode mode,
      FilterSet filters,
      Guid cutFillDesignID, //DesignDescriptor ACutFillDesign,
      //AReferenceVolumeType : TComputeICVolumesType;
      //AICOptions: TSVOICOptions;
      string requestingTRexNodeId,
      int dataPatchPageNumber,
      int dataPatchPageSize
    )
    {
      DataModelID = dataModelID;
      // ExternalDescriptor = AExternalDescriptor
      Mode = mode;
      Filters = filters;
      CutFillDesignID = cutFillDesignID; // CutFillDesign = ACutFillDesign;
      //ReferenceVolumeType = AReferenceVolumeType;
      //ICOptions = AICOptions;
      RequestingTRexNodeID = requestingTRexNodeId;
      DataPatchPageNumber = dataPatchPageNumber;
      DataPatchPageSize = dataPatchPageSize;
    }

    /// <summary>
    /// Executor that implements requesting and rendering subgrid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Guid[] SurveyedSurfaceExclusionList = new Guid[0];

      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}");

      ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

      RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

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
        PatchSubGridsResponse.ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // TODO: Or there was no data in the model
        return false;
      }

      // Get the current production data existance map from the sitemodel
      ProdDataExistenceMap = SiteModel.GetProductionDataExistanceMap(SiteModels.SiteModels.Instance().ImmutableStorageProxy);

      if (ProdDataExistenceMap == null)
      {
        PatchSubGridsResponse.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
        return false;
      }

      // Obtain the subgrid existence map for the project
      // Retrieve the existence map for the datamodel
      OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask()
      {
        CellSize = SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize
      };

      if (Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode))
      {
        // Obtain local reference to surveyed surfaces (lock free access)
        SurveyedSurfaces LocalSurveyedSurfaces = SiteModel.SurveyedSurfaces;
        SurveyedSurfaces Filter1SurveyedSurfaces = new SurveyedSurfaces();
        SurveyedSurfaces Filter2SurveyedSurfaces = new SurveyedSurfaces();

        if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(DataModelID, LocalSurveyedSurfaces, Filters.Filters[0],
          Filter2SurveyedSurfaces, Filter1SurveyedSurfaces, OverallExistenceMap))
        {
          PatchSubGridsResponse.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
          return false;
        }

        if (Filters.Filters.Length > 1)
        {
          if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(DataModelID, LocalSurveyedSurfaces, Filters.Filters[1],
            Filter1SurveyedSurfaces, Filter2SurveyedSurfaces, OverallExistenceMap))
          {
            PatchSubGridsResponse.ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
            return false;
          }
        }

        SurveyedSurfacesExludedViaTimeFiltering = !(Filter1SurveyedSurfaces.Count > 0 || Filter2SurveyedSurfaces.Count > 0);
      }

      OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

      foreach (var filter in Filters.Filters)
      {
        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel.ID, filter, OverallExistenceMap))
        {
          PatchSubGridsResponse.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        if (filter?.AttributeFilter.AnyFilterSelections == true)
        {
          PatchSubGridsResponse.ResultStatus = FilterUtilities.PrepareFilterForUse(filter, DataModelID);
          if (PatchSubGridsResponse.ResultStatus != RequestErrorStatus.OK)
            return false;
        }
      }

      Filters.ApplyFilterAndSubsetBoundariesToExtents(ref SpatialExtents);

      // If this request involves a relationship with a design then ensure the existance map
      // for the design is loaded in to memory to allow the request pipeline to confine
      // subgrid requests that overlay the actual design
      if (Utilities.RequestRequiresAccessToDesignFileExistanceMap(Mode /*, ReferenceVolumeType*/))
      {
        /*if (CutFillDesign.IsNull)
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
          PatchSubGridsResponse.ResultStatus = RequestErrorStatus.NoDesignProvided;
          return false;
        }

        DesignSubgridOverlayMap.CellSize = SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize;
      }

      // Execute the pipeline to query the subgrids and process then against the task
      if (ExecutePipeline())
        PatchSubGridsResponse.SubGrids = PipelinedTask.PatchSubgrids;

      return true;
    }

    private void ConfigurePipeline()
    {
      PipeLine.RequestDescriptor = RequestDescriptor;

      // PipeLine.ExternalDescriptor  = ExternalDescriptor;
      // PipeLine.SubmissionNode.DescriptorType  = cdtWMSTile;
      // PipeLine.TimeToLiveSeconds = VLPDSvcLocations.VLPDPSNode_TilePipelineTTLSeconds;

      PipeLine.DataModelID = DataModelID;

      // PipeLine.LiftBuildSettings  = FICOptions.GetLiftBuildSettings(FFilter1.LayerMethod);

      // If summaries of compaction information (both CMV and MDP) are being displayed,
      // and the lift build settings requires all layers to be examined (so the
      // apropriate summarize top layer only flag is false), then instruct the layer
      // analysis engine to apply to restriction to the number of cell passes to use
      // to perform layer analysis (ie: all cell passes will be used).

      if (Mode == DisplayMode.CCVSummary || Mode == DisplayMode.CCVPercentSummary)
      {
          /* TODO...
           if (!PipeLine.LiftBuildSettings.CCVSummarizeTopLayerOnly)
              PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
          */
      }

      if (Mode == DisplayMode.MDPSummary || Mode == DisplayMode.MDPPercentSummary)
      {
          /* TODO...
           if (!PipeLine.LiftBuildSettings.MDPSummarizeTopLayerOnly)
              PipeLine.MaxNumberOfPassesToReturn = VLPDSvcLocations.VLPDASNode_MaxCellPassDepthForAllLayersCompactionSummaryAnalysis;
          */
      }      

      PipeLine.OverallExistenceMap = OverallExistenceMap;
      PipeLine.ProdDataExistenceMap = ProdDataExistenceMap;
      PipeLine.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

      PipeLine.GridDataType = GridDataFromModeConverter.Convert(Mode);

      // Assign the filter set into the pipeline
      PipeLine.FilterSet = Filters;

      PipeLine.WorldExtents.Assign(SpatialExtents);

      PipeLine.IncludeSurveyedSurfaceInformation = Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) && !SurveyedSurfacesExludedViaTimeFiltering;
      if (PipeLine.IncludeSurveyedSurfaceInformation)  // if required then check if filter turns off requirement due to filters used
      {
        PipeLine.IncludeSurveyedSurfaceInformation = Utilities.FilterRequireSurveyedSurfaceInformation(PipeLine.FilterSet);
      }

      //PipeLine.NoChangeVolumeTolerance  = FICOptions.NoChangeVolumeTolerance;
    }

    /// <summary>
    /// Constructs and executes the pipeline to retrieve and operate on the filtered subgrids
    /// </summary>
    /// <returns></returns>
    protected bool ExecutePipeline()
    {
      bool PipelineAborted = false;
      // bool ShouldAbortDueToCompletedEventSet  = false;

      try
      {
        PipelinedTask = new PatchTask(RequestDescriptor, RequestingTRexNodeID, GridDataFromModeConverter.Convert(Mode));

        try
        {
          PipeLine = new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(PipelinedTask);
          PipelinedTask.PipeLine = PipeLine;

          ConfigurePipeline(/*out BoundingIntegerExtent2D CellExtents*/);

          // Provide the pipeline with a customised request analyser configured to return a specific page of subgrids
          PipeLine.RequestAnalyser = new RequestAnalyser(PipeLine, SpatialExtents)
          {
            SinglePageRequestNumber = DataPatchPageNumber,
            SinglePageRequestSize = DataPatchPageSize,
            SubmitSinglePageOfRequests = true
          };

          // If this is the first page requested then make a count the total number of patches required for all subgrids to be returned
          if (DataPatchPageNumber == 0)
          {
            PatchSubGridsResponse.TotalNumberOfPagesToCoverFilteredData =
              (int)Math.Truncate(Math.Ceiling(PipeLine.RequestAnalyser.CountOfSubgridsThatWillBeSubmitted() / (1.0 * DataPatchPageSize)));
          }

          if (PipeLine.Initiate())
            PipeLine.WaitForCompletion();

          PipelineAborted = PipeLine.Aborted;

          if (!PipeLine.Terminated && !PipeLine.Aborted)
            PatchSubGridsResponse.ResultStatus = RequestErrorStatus.OK;
        }
        finally
        {
          if (AbortedDueToTimeout)
            PatchSubGridsResponse.ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
          else
          {
            if (PipelinedTask.IsCancelled || PipelineAborted)
              PatchSubGridsResponse.ResultStatus = RequestErrorStatus.RequestHasBeenCancelled;

            //  ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.ASTasks.Remove(PipelinedTask);
          }
        }
      }
      catch (Exception E)
      {
        Log.LogError($"ExecutePipeline raised exception {E}");
        return false;
      }

      return PatchSubGridsResponse.ResultStatus == RequestErrorStatus.OK;
    }
  }
}

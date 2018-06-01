using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.Executors.Tasks;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics
{
    /// <summary>
    /// The base class the implements the analytics computation framework 
    /// </summary>
    public class AnalyticsComputor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The Aggregator to use for calculation of analytics
        /// </summary>
        public ISubGridRequestsAggregator Aggregator { get; set; }

        /// <summary>
        /// The Sitemodel from which the volume is being calculated
        /// </summary>
        public ISiteModel SiteModel { get; set; }

        /// <summary>
        /// Identifier for the design to be used as the basis for any required cut fill operations
        /// </summary>
        public Guid CutFillDesignID { get; set; } = Guid.Empty;

        /// <summary>
        /// The underlying grid data type required to satisfy the processing requirements of this analytics computor
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public AnalyticsComputor()
        {
        }

        public Guid RequestDescriptor { get; set; } = Guid.Empty;

        public FilterSet Filters { get; set; }

        public bool AbortedDueToTimeout { get; set; } = false;

        public bool IncludeSurveyedSurfaces { get; set; }

      /// <summary>
      /// Primary method called to begin analytics computation
      /// </summary>
      /// <returns></returns>
      public bool ComputeAnalytics(BaseAnalyticsResponse response)
      {
        try
        {
          // TODO: add when lift build setting ssupported
          // FAggregateState.LiftBuildSettings := FLiftBuildSettings;

          // Compute the report as required
          PipelineProcessor processor =
            new PipelineProcessor(requestDescriptor: RequestDescriptor,
              dataModelID: SiteModel.ID,
              gridDataType: RequestedGridDataType,
              response: response,
              filters: Filters,
              cutFillDesignID: CutFillDesignID,
              task: new AggregatedPipelinedSubGridTask(Aggregator),
              pipeline: new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>(),
              requestAnalyser: new RequestAnalyser(),
              requestRequiresAccessToDesignFileExistanceMap: CutFillDesignID != Guid.Empty,
              requireSurveyedSurfaceInformation: IncludeSurveyedSurfaces
            );

          if (!processor.Build())
          {
            Log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
            return false;
          }

          processor.Process();

          return response.ResultStatus == RequestErrorStatus.OK;
        }
        catch (Exception E)
        {
          Log.LogError($"ExecutePipeline raised exception: {E}");
        }

        return false;
      }
    }
}

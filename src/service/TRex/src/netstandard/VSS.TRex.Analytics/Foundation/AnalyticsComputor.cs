using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.Foundation
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
        /// together with its offset for a reference surface
        /// </summary>
        public DesignOffset CutFillDesign { get; set; } = new DesignOffset();

        /// <summary>
        /// The underlying grid data type required to satisfy the processing requirements of this analytics computor
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        /// Parameters for lift analysis
        /// </summary>
        public ILiftParameters LiftParams { get; set; } = new LiftParameters();

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public AnalyticsComputor()
        {
        }

        public Guid RequestDescriptor { get; set; } = Guid.Empty;

        public IFilterSet Filters { get; set; }

        public bool IncludeSurveyedSurfaces { get; set; }

        /// <summary>
        /// Primary method called to begin analytics computation
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ComputeAnalytics(BaseAnalyticsResponse response)
        {
          var processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
            RequestDescriptor,
            SiteModel.ID,
            RequestedGridDataType,
            response,
            Filters,
            CutFillDesign,
            DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.AggregatedPipelined),
            DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultAggregative),
            DIContext.Obtain<IRequestAnalyser>(),
            IncludeSurveyedSurfaces,
            CutFillDesign?.DesignID != Guid.Empty,
            BoundingIntegerExtent2D.Inverted(),
            LiftParams
          );

          // Assign the provided aggregator into the pipelined sub grid task
          ((IAggregatedPipelinedSubGridTask) processor.Task).Aggregator = Aggregator;

          if (!await processor.BuildAsync())
          {
            Log.LogError($"Failed to build pipeline processor for request to model {SiteModel.ID}");
            return false;
          }

          processor.Process();

          return response.ResultStatus == RequestErrorStatus.OK;
        }
    }
}

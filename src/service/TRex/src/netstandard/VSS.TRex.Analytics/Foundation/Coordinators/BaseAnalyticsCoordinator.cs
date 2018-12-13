using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Analytics.Foundation.Coordinators
{
    /// <summary>
    /// Base class used by all Analytics style operations. It defines common state and behaviour for those requests 
    /// at the client context level.
    /// </summary>
    public abstract class BaseAnalyticsCoordinator<TArgument, TResponse> : IBaseAnalyticsCoordinator<TArgument, TResponse> where TArgument : BaseApplicationServiceRequestArgument
        where TResponse : BaseAnalyticsResponse, new()
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The SiteModel context for computing the result of the request
        /// </summary>
        public ISiteModel SiteModel { get; set; }

        /// <summary>
        /// Request descriptor used to track this request in different parts of the cluster compute
        /// </summary>
        public Guid RequestDescriptor { get; set; }

        /// <summary>
        /// Execution method for the derived coordinator to override
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public TResponse Execute(TArgument arg)
        {
            Log.LogInformation("In: Executing Coordination logic");

            TResponse Response = new TResponse();
            try
            {
                RequestDescriptor = Guid.NewGuid(); 

                SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);

                AggregatorBase Aggregator = ConstructAggregator(arg);
                AnalyticsComputor Computor = ConstructComputor(arg, Aggregator);

                if (Computor.ComputeAnalytics(Response))
                {
                    // Instruct the Aggregator to perform any finalisation logic before returning results
                    Aggregator.Finalise();

                    ReadOutResults(Aggregator, Response);
                }
            }
            catch (Exception E)
            {
                Log.LogError("Exception:", E);
            }

            Log.LogInformation("Out: Executing Coordination logic");

            return Response;
        }

        /// <summary>
        /// Constructs the aggregator to be used as the reduction function for the MapReduceReduce computation
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public abstract AggregatorBase ConstructAggregator(TArgument argument);

        /// <summary>
        /// Constructs the computor responsible for orchestrating information requests, essentially the map part of the MapReduceReduce computation
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="aggregator"></param>
        /// <returns></returns>
        public abstract AnalyticsComputor ConstructComputor(TArgument argument, AggregatorBase aggregator);

        /// <summary>
        /// Transcribes the results of the computation from the internal response type to the external response type
        /// </summary>
        /// <param name="aggregator"></param>
        /// <param name="response"></param>
        public abstract void ReadOutResults(AggregatorBase aggregator, TResponse response);
    }
}

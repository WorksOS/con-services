using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Analytics.GridFabric.Responses;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SiteModels;
using VSS.TRex.Types;
using VSS.TRex.Utilities;

namespace VSS.TRex.Analytics.Coordinators
{
    /// <summary>
    /// Base class used by all Analytics style operations. It defines common state and behaviour for those requests 
    /// at the client context level.
    /// </summary>
    public abstract class BaseAnalyticsCoordinator<TArgument, TResponse> : IBaseAnalyticsCoordinator<TArgument, TResponse> where TArgument : BaseApplicationServiceRequestArgument
        where TResponse : BaseAnalyticsResponse, new()
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        /// <summary>
        /// The SiteModel context for computing the result of the request
        /// </summary>
        public SiteModel SiteModel { get; set; }

        /// <summary>
        /// Request descriptor used to track this request in different parts of the cluster compute
        /// </summary>
        public long RequestDescriptor { get; set; }

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
                /* TODO ... Some concerns from various requests that may need ot be taken into account
                    //  ScheduledWithGovernor       :Boolean = false;
                    //  SurveyedSurfaceExclusionList:TSurveyedSurfaceIDList;

                        if Assigned(ASNodeImplInstance.RequestCancellations) and ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
                          begin
                            SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, slmcDebug);
                            ASNodeResult := asneRequestHasBeenCancelled;
                            Exit;
                          end;

                                SetLength(SurveyedSurfaceExclusionList, 0);

                              if ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetDataModelSpatialExtents(FDataModelID, SurveyedSurfaceExclusionList, SpatialExtent, CellSize, IndexOriginOffset) <> icsrrNoError then
                                begin
                                  ASNodeResult := asneFailedToRequestDatamodelStatistics;
                                  Exit;
                                end;

                        //BoundingWorldExtent3D ResultBoundingExtents = BoundingWorldExtent3D.Null();
                        //BoundingWorldExtent3D SpatialExtent = BoundingWorldExtent3D.Null();
                        //long[] SurveyedSurfaceExclusionList = new long[0];
                }
                */

                RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

                SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(arg.DataModelID);

                Response.ResultStatus = FilterUtilities.PrepareFilterForUse(arg.Filter, arg.DataModelID);
                if (Response.ResultStatus != RequestErrorStatus.OK)
                {
                    Log.LogInformation($"PrepareFilterForUse failed: Datamodel={arg.DataModelID}");
                    return Response;
                }

                AggregatorBase Aggregator = ConstructAggregator(arg);
                AnalyticsComputor Computor = ConstructComputor(arg, Aggregator);

                // TODO - Need to figure out where to put this in relevant queries
                // Reporter.LiftBuildSettings.Assign(FLiftBuildSettings);

                if (Computor.ComputeAnalytics())
                    Response.ResultStatus = RequestErrorStatus.OK;
                else if (Computor.AbortedDueToTimeout)
                    Response.ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
                else
                    Response.ResultStatus = RequestErrorStatus.Unknown;

                if (Response.ResultStatus == RequestErrorStatus.OK)
                {
                    // Instruct the Aggregator to perform any finalisation logic before returning results
                    Aggregator.Finalise();

                    ReadOutResults(Aggregator, Response);
                }
            }
            catch (Exception E)
            {
                Log.LogError($"Exception {E}");
            }

            Log.LogInformation("Out: Executing Coordination logic");

            return Response;
        }

        /// <summary>
        /// Constructs the aggegator to be used as the reduction function for the MapReduceReduce computation
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public abstract AggregatorBase ConstructAggregator(TArgument argument);

        /// <summary>
        /// Constructs the computer responsible for orchestrating information requests, essentially the map part of the MapReduceReduce computation
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
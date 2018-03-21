using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.Aggregators;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.Analytics.Coordinators
{
    /// <summary>
    /// Computes cut fill statistics. Executes in the 'application service' layer and acts as the coordinator
    /// for the request onto the cluster compute layer.
    /// </summary>
    public class CutFillCoordinator : BaseAnalyticsCoordinator<CutFillStatisticsArgument, CutFillStatisticsResponse>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Executes the cut fill analytics request returning the counts and percetages for the (7) defined cut fill bands
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override CutFillStatisticsResponse Execute(CutFillStatisticsArgument arg) 
        {
            Log.Info("In: Executing Coordination logic");

            //  ScheduledWithGovernor       :Boolean = false;
            //  SurveyedSurfaceExclusionList:TSurveyedSurfaceIDList;
            CutFillStatisticsResponse result = new CutFillStatisticsResponse();

            try
            {
                /* TODO...
                if not Assigned(ASNodeImplInstance) or ASNodeImplInstance.ServiceStopped then
                  begin
                    SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), slmcWarning);
                    Exit;
                  end;

                if Assigned(ASNodeImplInstance.RequestCancellations) and ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
                  begin
                    SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, slmcDebug);
                    ASNodeResult := asneRequestHasBeenCancelled;
                    Exit;
                  end;
          */

                // TODO - Readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('#In# Performing %s.Execute for DataModel:%d', [Self.ClassName, FDataModelID]), slmcMessage);

                /* TODO...
                 *ScheduledWithGovernor := ASNodeImplInstance.Governor.Schedule(FExternalDescriptor, Self, gqVolumes, ASNodeResult);
                      if not ScheduledWithGovernor then
                        Exit;

                        SetLength(SurveyedSurfaceExclusionList, 0);

                      if ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetDataModelSpatialExtents(FDataModelID, SurveyedSurfaceExclusionList, SpatialExtent, CellSize, IndexOriginOffset) <> icsrrNoError then
                        begin
                          ASNodeResult := asneFailedToRequestDatamodelStatistics;
                          Exit;
                        end;
                */

                //BoundingWorldExtent3D ResultBoundingExtents = BoundingWorldExtent3D.Null();
                //BoundingWorldExtent3D SpatialExtent = BoundingWorldExtent3D.Null();
                //long[] SurveyedSurfaceExclusionList = new long[0];

                long RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

                result.ResultStatus = FilterUtilities.PrepareFilterForUse(arg.Filter, arg.DataModelID);
                if (result.ResultStatus != RequestErrorStatus.OK)
                {
                    Log.Info($"PrepareFilterForUse failed: Datamodel={arg.DataModelID}");
                    return result;
                }

                // Obtain the site model context for the request
                SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(arg.DataModelID);

                // Create the aggregator to collect and reduce the results. As a part of this locate the
                // design instance representing the design the cut/fill information is being calculated against
                // and supply that to the aggregator
                CutFillAggregator Aggregator = new CutFillAggregator()
                {
                    RequiresSerialisation = true,
                    SiteModelID = arg.DataModelID,
                    //LiftBuildSettings := LiftBuildSettings;
                    CellSize = SiteModel.Grid.CellSize,
                    Offsets = arg.Offsets
                };

                // Create the analytics engine to orchestrate the calculation
                AnalyticsComputor<CutFillStatisticsArgument, CutFillStatisticsResponse> Computor = new AnalyticsComputor<CutFillStatisticsArgument, CutFillStatisticsResponse>()
                {
                    RequestDescriptor = RequestDescriptor,
                    SiteModel = SiteModel,
                    Aggregator = Aggregator,
                    Filter = arg.Filter,
                    IncludeSurveyedSurfaces = true,
                    RequestedGridDataType = GridDataType.CutFill, // TODO: Change to CutFill when this is merged from Legacy source                    
                    CutFillDesignID = arg.DesignID
                };

                // TODO Readd when logging available: 
                // Reporter.LiftBuildSettings.Assign(FLiftBuildSettings);

                if (Computor.ComputeAnalytics())
                    result.ResultStatus = RequestErrorStatus.OK;
                else if (Computor.AbortedDueToTimeout)
                    result.ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
                else
                    result.ResultStatus = RequestErrorStatus.Unknown;

                if (result.ResultStatus == RequestErrorStatus.OK)
                {
                    // Instruct the Aggregator to perform any finalisation logic before reading out the results
                    Aggregator.Finalise();
                    result.Counts = Aggregator.Counts;
                }
            }
            catch (Exception E)
            {
                Log.Error($"Exception {E}");
            }

            Log.Info("Out: Executing Coordination logic");

            return result;
        }
    }
}

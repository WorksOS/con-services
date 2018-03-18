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
using VSS.VisionLink.Raptor.Analytics.Models;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.Analytics.Operations
{
    /// <summary>
    /// Computes cut fill statistics. Executes in the 'application service' layer
    /// </summary>
    public class CutFill
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Details the error status of the bmp result returned by the renderer
        /// </summary>
        public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

        private void InitialiseComputor(AnalyticsComputor<CutFillStatisticsArgument, CutFillStatisticResponse> Computor)
        {

        }

        public CutFillResult Execute(CutFillStatisticsArgument arg) 
        {

            //  ScheduledWithGovernor       :Boolean = false;
            //  SpatialExtent               :T3DBoundingWorldExtent;
            //  CellSize                    :Double;
            //  IndexOriginOffset           :Integer;
            //  SurveyedSurfaceExclusionList:TSurveyedSurfaceIDList;
            //  ResultString                :String = '';

            CutFillResult result = new CutFillResult();

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

                //      RequestDescriptor := ASNodeImplInstance.NextDescriptor;

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

                BoundingWorldExtent3D ResultBoundingExtents = BoundingWorldExtent3D.Null();
                BoundingWorldExtent3D SpatialExtent = BoundingWorldExtent3D.Null();
                long[] SurveyedSurfaceExclusionList = new long[0];

                RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

                long RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

                ResultStatus = FilterUtilities.PrepareFilterForUse(arg.Filter, arg.DataModelID);
                if (ResultStatus != RequestErrorStatus.OK)
                {
                    return null;
                }

                // Obtain the site model context for the request
                SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(arg.DataModelID);

                CutFillAggregator Aggregator = new CutFillAggregator()
                {
                    RequiresSerialisation = true,
                    SiteModelID = arg.DataModelID,
                    //LiftBuildSettings := LiftBuildSettings;
                    CellSize = SiteModel.Grid.CellSize,
                    Offsets = arg.Offsets
                };

                // create reporting engine
                AnalyticsComputor<CutFillStatisticsArgument, CutFillStatisticResponse> Computor = new AnalyticsComputor<CutFillStatisticsArgument, CutFillStatisticResponse>()
                {
                    RequestDescriptor = RequestDescriptor,
                    SiteModel = SiteModel,
                    Aggregator = Aggregator,
                    Filter = arg.Filter,
                    IncludeSurveyedSurfaces = true,
                    RequestedGridDataType = GridDataType.Height, // TODO: Change to CutFIll when this is merged from Legacy source                    
                    CutFillDesignID = arg.DesignID
                };

                InitialiseComputor(Computor);

                //Reporter.AggregateState.CutFillSettings = FCutFillSettings; // Has Design Descriptor

                // Readd when logging available
                // Reporter.LiftBuildSettings.Assign(FLiftBuildSettings);

                if (Computor.ComputeAnalytics())
                    ResultStatus = RequestErrorStatus.OK;
                else
                if (Computor.AbortedDueToTimeout)
                    ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
                else
                    ResultStatus = RequestErrorStatus.Unknown;

                if (ResultStatus != RequestErrorStatus.OK)
                {
                    // Send the (emnpty) results back to the caller
                    return result;
                }

                // Instruct the Aggregator to perform any finalisation logic before reading out the results
                Aggregator.Finalise();

                result.Counts = Aggregator.Counts;
            }
            catch (Exception E)
            {
                Log.Error($"Exception {E}");
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Common.Executors;
using MasterDataProxies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using src.Interfaces;
using src.Models;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Compaction.Helpers;
using VSS.Raptor.Service.WebApiModels.Compaction.Models;
using VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes;
using VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Executors;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;
using WebApiModels.Compaction.Executors;
using WebApiModels.Compaction.Models;
using WebApiModels.Notification.Helpers;
using ColorValue = VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes.ColorValue;

namespace VSS.Raptor.Service.WebApi.Compaction.Controllers
{

    [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
    public class CompactionController : Controller
    {
        /// <summary>
        /// Raptor client for use by executor
        /// </summary>
        private readonly IASNodeClient raptorClient;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// Logger factory for use by executor
        /// </summary>
        private readonly ILoggerFactory logger;

        /// <summary>
        /// Used to get list of projects for customer
        /// </summary>
        private readonly IAuthenticatedProjectsStore authProjectsStore;

        /// <summary>
        /// Cache for elevation extents, needed for elevation palette
        /// </summary>
        private readonly IMemoryCache elevationExtentsCache;

        /// <summary>
        /// How long to cache elevation extents
        /// </summary>
        private readonly TimeSpan elevationExtentsCacheLife = new TimeSpan(0, 15, 0); //TODO: how long to cache ?

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    /// <param name="cache">Elevation extents cache</param>
    /// <param name="fileListProxy">File list proxy</param>
    public CompactionController(IASNodeClient raptorClient, ILoggerFactory logger,
            IAuthenticatedProjectsStore authProjectsStore, IMemoryCache cache, IFileListProxy fileListProxy)
        {
            this.raptorClient = raptorClient;
            this.logger = logger;
            this.log = logger.CreateLogger<CompactionController>();
            this.authProjectsStore = authProjectsStore;
            this.elevationExtentsCache = cache;
            this.fileListProxy = fileListProxy;
        }

        /// <summary>
        /// Creates an instance of the CMVRequest class and populate it with data.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectUid"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <param name="vibeStateOn"></param>
        /// <param name="elevationType"></param>
        /// <param name="layerNumber"></param>
        /// <param name="onMachineDesignId"></param>
        /// <param name="assetID"></param>
        /// <param name="machineName"></param>
        /// <param name="isJohnDoe"></param>
        /// <returns>An instance of the CMVRequest class.</returns>
        private CMVRequest GetCMVRequest(long? projectId, Guid? projectUid, DateTime? startUtc, DateTime? endUtc,
            bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, long? onMachineDesignId, long? assetID,
            string machineName, bool? isJohnDoe)
        {
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            CMVSettings cmvSettings = CompactionSettings.CompactionCmvSettings;
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));

            return CMVRequest.CreateCMVRequest(projectId.Value, null, cmvSettings, liftSettings, filter, -1, null, null,
                null);
        }

        /// <summary>
        /// Creates an instance of the PassCounts class and populate it with data.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectUid"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <param name="vibeStateOn"></param>
        /// <param name="elevationType"></param>
        /// <param name="layerNumber"></param>
        /// <param name="onMachineDesignId"></param>
        /// <param name="assetID"></param>
        /// <param name="machineName"></param>
        /// <param name="isJohnDoe"></param>
        /// <param name="isSummary"></param>
        /// <returns>An instance of the PassCounts class.</returns>
        private PassCounts GetPassCountRequest(long? projectId, Guid? projectUid, DateTime? startUtc, DateTime? endUtc,
            bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, long? onMachineDesignId, long? assetID,
            string machineName, bool? isJohnDoe, bool isSummary)
        {
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            PassCountSettings passCountSettings = isSummary ? null : CompactionSettings.CompactionPassCountSettings;
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));

            return PassCounts.CreatePassCountsRequest(projectId.Value, null, passCountSettings, liftSettings, filter,
                -1, null, null, null);
        }

        #region Summary Data for Widgets

        /// <summary>
        /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>CMV summary</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/cmv/summary")]
        [HttpGet]
        public CompactionCmvSummaryResult GetCmvSummary(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetCmvSummary: " + Request.QueryString);

            CMVRequest request = GetCMVRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn, elevationType,
                layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe);
            request.Validate();

            try
            {
                var result =
                    RequestExecutorContainer.Build<SummaryCMVExecutor>(logger, raptorClient, null).Process(request) as
                        CMVSummaryResult;
                var returnResult = CompactionCmvSummaryResult.CreateCmvSummaryResult(result, request.cmvSettings);
                log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetCmvSummary returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Get MDP summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>MDP summary</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/mdp/summary")]
        [HttpGet]
        public CompactionMdpSummaryResult GetMdpSummary(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetMdpSummary: " + Request.QueryString);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            MDPSettings mdpSettings = CompactionSettings.CompactionMdpSettings;
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));
            MDPRequest request = MDPRequest.CreateMDPRequest(projectId.Value, null, mdpSettings, liftSettings, filter,
                -1,
                null, null, null);
            request.Validate();
            try
            {
                var result = RequestExecutorContainer.Build<SummaryMDPExecutor>(logger, raptorClient, null)
                    .Process(request) as MDPSummaryResult;
                var returnResult = CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
                log.LogInformation("GetMdpSummary result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetMdpSummary returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Get pass count summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>Pass count summary</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/passcounts/summary")]
        [HttpGet]
        public CompactionPassCountSummaryResult GetPassCountSummary(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetPassCountSummary: " + Request.QueryString);

            PassCounts request = GetPassCountRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn,
                elevationType, layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe, true);
            request.Validate();

            try
            {
                var result = RequestExecutorContainer.Build<SummaryPassCountsExecutor>(logger, raptorClient, null)
                    .Process(request) as PassCountSummaryResult;
                var returnResult = CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
                log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetPassCountSummary returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Get Temperature summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>Temperature summary</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/temperature/summary")]
        [HttpGet]
        public CompactionTemperatureSummaryResult GetTemperatureSummary(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetTemperatureSummary: " + Request.QueryString);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            TemperatureSettings temperatureSettings = CompactionSettings.CompactionTemperatureSettings;
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));

            TemperatureRequest request = TemperatureRequest.CreateTemperatureRequest(projectId.Value, null,
                temperatureSettings, liftSettings, filter, -1,
                null, null, null);
            request.Validate();
            try
            {
                var result =
                    RequestExecutorContainer.Build<SummaryTemperatureExecutor>(logger, raptorClient, null)
                        .Process(request) as TemperatureSummaryResult;
                var returnResult = CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(result);
                log.LogInformation("GetTemperatureSummary result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetTemperatureSummary returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Get Speed summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>Speed summary</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/speed/summary")]
        [HttpGet]
        public CompactionSpeedSummaryResult GetSpeedSummary(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetSpeedSummary: " + Request.QueryString);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            //Speed settings are in LiftBuildSettings
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));

            SummarySpeedRequest request =
                SummarySpeedRequest.CreateSummarySpeedRequestt(projectId.Value, null, liftSettings, filter, -1);
            request.Validate();
            try
            {
                var result = RequestExecutorContainer.Build<SummarySpeedExecutor>(logger, raptorClient, null)
                    .Process(request) as SummarySpeedResult;
                var returnResult =
                    CompactionSpeedSummaryResult.CreateSpeedSummaryResult(result, liftSettings.machineSpeedTarget);
                log.LogInformation("GetSpeedSummary result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetSpeedSummary returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Get CMV % change from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>CMV % change</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/cmv/percentchange")]
        [HttpGet]
        public CompactionCmvPercentChangeResult GetCmvPercentChange(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetCmvPercentChange: " + Request.QueryString);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));
            double[] cmvChangeSummarySettings = CompactionSettings.CompactionCmvPercentChangeSettings;
            CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(
                projectId.Value, null, liftSettings, filter, -1, cmvChangeSummarySettings);
            request.Validate();
            try
            {
                var result = RequestExecutorContainer.Build<CMVChangeSummaryExecutor>(logger, raptorClient, null)
                    .Process(request) as CMVChangeSummaryResult;
                var returnResult = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result);
                log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);

                throw;
            }
            finally
            {
                log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
            }
        }

        #endregion

        #region Detailed Data for the map

        /// <summary>
        /// Get CMV details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>CMV details</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/cmv/details")]
        [HttpGet]
        public CompactionCmvDetailedResult GetCmvDetails(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetCmvDetails: " + Request.QueryString);

            CMVRequest request = GetCMVRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn, elevationType,
                layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe);
            request.Validate();

            try
            {
                var result = RequestExecutorContainer.Build<DetailedCMVExecutor>(logger, raptorClient, null)
                    .Process(request) as CMVDetailedResult;
                var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result);

                log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetCmvDetails returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Get pass count details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>Pass count details</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/passcounts/details")]
        [HttpGet]
        public CompactionPassCountDetailedResult GetPassCountDetails(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetPassCountDetails: " + Request.QueryString);

            PassCounts request = GetPassCountRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn,
                elevationType, layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe, false);
            request.Validate();

            try
            {
                var result = RequestExecutorContainer.Build<DetailedPassCountExecutor>(logger, raptorClient, null)
                    .Process(request) as PassCountDetailedResult;
                var returnResult = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(result);
                log.LogInformation("GetPassCountDetails result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetPassCountDetails returned: " + Response.StatusCode);
            }
        }

        #endregion

        #region Palettes

        /// <summary>
        /// Get color palettes for a project.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <returns>Color palettes for all display types</returns>
        [Route("api/v2/compaction/colorpalettes")]
        [HttpGet]
        public CompactionColorPalettesResult GetColorPalettes([FromQuery] long? projectId, [FromQuery] Guid? projectUid)
        {
            log.LogInformation("GetColorPalettes: " + Request.QueryString);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            List<DisplayMode> modes = new List<DisplayMode>
            {
                DisplayMode.Height,
                DisplayMode.CCV,
                DisplayMode.PassCount,
                DisplayMode.PassCountSummary,
                DisplayMode.CutFill,
                DisplayMode.TemperatureSummary,
                DisplayMode.CCVPercentSummary,
                DisplayMode.MDPPercentSummary,
                DisplayMode.TargetSpeedSummary,
                DisplayMode.CMVChange
            };

            DetailPalette elevationPalette = null;
            DetailPalette cmvDetailPalette = null;
            DetailPalette passCountDetailPalette = null;
            SummaryPalette passCountSummaryPalette = null;
            DetailPalette cutFillPalette = null;
            SummaryPalette temperatureSummaryPalette = null;
            SummaryPalette cmvSummaryPalette = null;
            SummaryPalette mdpSummaryPalette = null;
            DetailPalette cmvPercentChangePalette = null;
            SummaryPalette speedSummaryPalette = null;

            //This is temporary until temperature details implemented in Raptor.
            DetailPalette temperatureDetailPalette = DetailPalette.CreateDetailPalette(
                new List<ColorValue>
                {
                    ColorValue.CreateColorValue(0x2D5783, 70),
                    ColorValue.CreateColorValue(0x439BDC, 80),
                    ColorValue.CreateColorValue(0xBEDFF1, 90),
                    ColorValue.CreateColorValue(0xDCEEC7, 100),
                    ColorValue.CreateColorValue(0x9DCE67, 110),
                    ColorValue.CreateColorValue(0x6BA03E, 120),
                    ColorValue.CreateColorValue(0x3A6B25, 130),
                    ColorValue.CreateColorValue(0xF6CED3, 140),
                    ColorValue.CreateColorValue(0xD57A7C, 150),
                    ColorValue.CreateColorValue(0xC13037, 160)
                },
                null, null);


            foreach (var mode in modes)
            {
                List<ColorValue> colorValues;
                ElevationStatisticsResult elevExtents = mode == DisplayMode.Height
                    ? GetElevationRange(projectId.Value, null)
                    : null;
                var compactionPalette = CompactionSettings.CompactionPalette(mode, elevExtents);
                switch (mode)
                {
                    case DisplayMode.Height:
                        colorValues = new List<ColorValue>();
                        for (int i = 1; i < compactionPalette.Count - 1; i++)
                        {
                            colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                                compactionPalette[i].value));
                        }
                        elevationPalette = DetailPalette.CreateDetailPalette(colorValues,
                            compactionPalette[compactionPalette.Count - 1].color, compactionPalette[0].color);
                        break;
                    case DisplayMode.CCV:
                        colorValues = new List<ColorValue>();
                        for (int i = 0; i < compactionPalette.Count; i++)
                        {
                            colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                                compactionPalette[i].value));
                        }
            cmvDetailPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
                        break;
                    case DisplayMode.PassCount:
                        colorValues = new List<ColorValue>();
                        for (int i = 0; i < compactionPalette.Count - 1; i++)
                        {
                            colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                                compactionPalette[i].value));
                        }
                        passCountDetailPalette = DetailPalette.CreateDetailPalette(colorValues,
                            compactionPalette[compactionPalette.Count - 1].color, null);
                        break;
                    case DisplayMode.PassCountSummary:
                        passCountSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].color,
                            compactionPalette[1].color, compactionPalette[0].color);
                        break;
                    case DisplayMode.CutFill:
                        colorValues = new List<ColorValue>();
                        for (int i = compactionPalette.Count - 1; i >= 0; i--)
                        {
                            colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                                compactionPalette[i].value));
                        }
                        cutFillPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
                        break;
                    case DisplayMode.TemperatureSummary:
                        temperatureSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].color,
                            compactionPalette[1].color, compactionPalette[0].color);
                        break;
                    case DisplayMode.CCVPercentSummary:
                        cmvSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[3].color,
                            compactionPalette[0].color, compactionPalette[2].color);
                        break;
                    case DisplayMode.MDPPercentSummary:
                        mdpSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[3].color,
                            compactionPalette[0].color, compactionPalette[2].color);
                        break;
                    case DisplayMode.TargetSpeedSummary:
                        speedSummaryPalette = SummaryPalette.CreateSummaryPalette(compactionPalette[2].color,
                            compactionPalette[1].color, compactionPalette[0].color);
                        break;
                    case DisplayMode.CMVChange:
                        colorValues = new List<ColorValue>();
                        for (int i = 1; i < compactionPalette.Count - 1; i++)
                        {
                            colorValues.Add(ColorValue.CreateColorValue(compactionPalette[i].color,
                                compactionPalette[i].value));
                        }
                        cmvPercentChangePalette = DetailPalette.CreateDetailPalette(colorValues,
                            compactionPalette[compactionPalette.Count - 1].color, compactionPalette[0].color);
                        break;
                }

            }
            return CompactionColorPalettesResult.CreateCompactionColorPalettesResult(
                elevationPalette, cmvDetailPalette, passCountDetailPalette, passCountSummaryPalette, cutFillPalette,
                temperatureSummaryPalette,
                cmvSummaryPalette, mdpSummaryPalette, cmvPercentChangePalette, speedSummaryPalette,
                temperatureDetailPalette);
        }

        #endregion

        #region Elevation Range

        /// <summary>
        /// Get elevation range from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>Elevation statistics</returns>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/elevationrange")]
        [HttpGet]
        public ElevationStatisticsResult GetElevationRange(
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogInformation("GetElevationRange: " + Request.QueryString);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            try
            {
                Filter filter = CompactionSettings.CompactionFilter(
                    startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                    GetMachines(assetID, machineName, isJohnDoe));
                ElevationStatisticsResult result = GetElevationRange(projectId.Value, filter);
                log.LogInformation("GetElevationRange result: " + JsonConvert.SerializeObject(result));
                return result;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("GetElevationRange returned: " + Response.StatusCode);
            }
        }

        /// <summary>
        /// Gets the elevation statistics for the given filter
        /// </summary>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="filter">Compaction filter</param>
        /// <param name="cacheKey">Elevation extents cache key</param>
        /// <returns>Elevation statistics</returns>
        private ElevationStatisticsResult GetElevationRange(long projectId, Filter filter)
        {
            ElevationStatisticsResult result = null;
            string cacheKey = ElevationCacheKey(projectId, filter);
            if (!this.elevationExtentsCache.TryGetValue(cacheKey, out result))
            {
                LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;

                ElevationStatisticsRequest statsRequest =
                    ElevationStatisticsRequest.CreateElevationStatisticsRequest(projectId, null, filter, 0,
                        liftSettings);
                statsRequest.Validate();

                result =
                    RequestExecutorContainer.Build<ElevationStatisticsExecutor>(logger, raptorClient, null)
                        .Process(statsRequest) as ElevationStatisticsResult;

                var opts = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = elevationExtentsCacheLife
                };
                elevationExtentsCache.Set(cacheKey, result, opts);
            }
            return result;
        }

        /// <summary>
        /// Gets the key for the elevation extents cache
        /// </summary>
        /// <param name="projectId">project ID</param>
        /// <param name="filter">Compaction filter</param>
        /// <returns>Cache key</returns>
        private string ElevationCacheKey(long projectId, Filter filter)
        {
            return
                filter == null
                    ? ElevationCacheKey(projectId, null, null, null, null, null, null, null, null, null)
                    : ElevationCacheKey(projectId, filter.startUTC, filter.endUTC, filter.vibeStateOn,
                        filter.elevationType, filter.layerNumber, filter.onMachineDesignID,
                        filter.contributingMachines == null || filter.contributingMachines.Count == 0
                            ? (long?) null
                            : filter.contributingMachines[0].assetID,
                        //Can only filter by one machine at present
                        filter.contributingMachines == null || filter.contributingMachines.Count == 0
                            ? null
                            : filter.contributingMachines[0].machineName,
                        filter.contributingMachines == null || filter.contributingMachines.Count == 0
                            ? (bool?) null
                            : filter.contributingMachines[0].isJohnDoe);
        }

        /// <summary>
        /// Gets the key for the elevation extents cache
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <param name="vibeStateOn"></param>
        /// <param name="elevationType"></param>
        /// <param name="layerNumber"></param>
        /// <param name="onMachineDesignId"></param>
        /// <param name="assetID"></param>
        /// <param name="machineName"></param>
        /// <param name="isJohnDoe"></param>
        /// <returns>Cache key</returns>
        private string ElevationCacheKey(long projectId, DateTime? startUtc, DateTime? endUtc,
            bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, long? onMachineDesignId, long? assetID,
            string machineName, bool? isJohnDoe)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", projectId, startUtc, endUtc, vibeStateOn,
                elevationType, layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe);
        }

        #endregion

        // TEMP v2 copy of v1 until we have a simplified contract for Compaction
        /// <summary>
        /// Gets project statistics from Raptor.
        /// </summary>
        /// <param name="request">The request for statistics request to Raptor</param>
        /// <returns></returns>
        /// <executor>ProjectStatisticsExecutor</executor>
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/projectstatistics")]
        [HttpPost]
        public ProjectStatisticsResult PostProjectStatistics([FromBody] ProjectStatisticsRequest request)
        {
            log.LogInformation("PostProjectStatistics: " + JsonConvert.SerializeObject(request));
            request.Validate();
            try
            {
                var returnResult =
                    RequestExecutorContainer.Build<ProjectStatisticsExecutor>(logger, raptorClient, null)
                            .Process(request)
                        as ProjectStatisticsResult;
                log.LogInformation("PostProjectStatistics result: " + JsonConvert.SerializeObject(returnResult));
                return returnResult;
            }
            catch (ServiceException se)
            {
                //Change FailedToGetResults to 204
                ProcessStatusCode(se);
                throw;
            }
            finally
            {
                log.LogInformation("PostProjectStatistics returned: " + Response.StatusCode);
            }
        }

        private void ProcessStatusCode(ServiceException se)
        {
            if (se.Code == HttpStatusCode.BadRequest &&
                se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
            {
                se.Code = HttpStatusCode.NoContent;
            }
        }


        #region Tiles

        /// <summary>
        /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
        /// elevation, compaction, temperature, cut/fill, volumes etc
        /// </summary>
        /// <param name="request">A representation of the tile rendering request.</param>
        /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
        /// <executor>TilesExecutor</executor> 
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/productiondatatiles")]
        [HttpPost]
        public TileResult PostTile([FromBody] CompactionTileRequest request)
        {
            log.LogDebug("PostTile: " + JsonConvert.SerializeObject(request));
            request.Validate();

            Filter filter = request.filter == null
                ? null
                : CompactionSettings.CompactionFilter(
                    request.filter.startUTC, request.filter.endUTC, request.filter.onMachineDesignID,
                    request.filter.vibeStateOn,
                    request.filter.elevationType, request.filter.layerNumber, request.filter.contributingMachines);
            var tileResult = GetProductionDataTile(filter, request.projectId.Value, request.mode, request.width, request.height,
                request.boundBoxLL);
            return tileResult;
        }


        /// <summary>
        /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
        /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
        /// </summary>
        /// <param name="request">A representation of the tile rendering request.</param>
        /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
        /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
        /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
        /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
        /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
        /// </returns>
        /// <executor>TilesExecutor</executor> 
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/productiondatatiles/png")]
        [HttpPost]

        public FileResult PostTileRaw([FromBody] CompactionTileRequest request)
        {
            log.LogDebug("PostTileRaw: " + JsonConvert.SerializeObject(request));
            request.Validate();

            Filter filter = request.filter == null
                ? null
                : CompactionSettings.CompactionFilter(
                    request.filter.startUTC, request.filter.endUTC, request.filter.onMachineDesignID,
                    request.filter.vibeStateOn,
                    request.filter.elevationType, request.filter.layerNumber, request.filter.contributingMachines);
            var tileResult = GetProductionDataTile(filter, request.projectId.Value, request.mode, request.width, request.height,
                request.boundBoxLL);
            if (tileResult != null)
            {
                Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());
                AddCacheResponseHeaders();               
                return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
            }

            throw new ServiceException(HttpStatusCode.NoContent,
                new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                    "Raptor failed to return a tile"));
        }

        /// <summary>
        /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
        /// elevation, compaction, temperature, cut/fill, volumes etc
        /// </summary>
        /// <param name="SERVICE">WMS parameter - value WMS</param>
        /// <param name="VERSION">WMS parameter - value 1.3.0</param>
        /// <param name="REQUEST">WMS parameter - value GetMap</param>
        /// <param name="FORMAT">WMS parameter - value image/png</param>
        /// <param name="TRANSPARENT">WMS parameter - value true</param>
        /// <param name="LAYERS">WMS parameter - value Layers</param>
        /// <param name="CRS">WMS parameter - value EPSG:4326</param>
        /// <param name="STYLES">WMS parameter - value null</param>
        /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
        /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
        /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
        /// <executor>TilesExecutor</executor> 
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/productiondatatiles")]
        [HttpGet]
        public TileResult GetProductionDataTile(
            [FromQuery] string SERVICE,
            [FromQuery] string VERSION,
            [FromQuery] string REQUEST,
            [FromQuery] string FORMAT,
            [FromQuery] string TRANSPARENT,
            [FromQuery] string LAYERS,
            [FromQuery] string CRS,
            [FromQuery] string STYLES,
            [FromQuery] int WIDTH,
            [FromQuery] int HEIGHT,
            [FromQuery] string BBOX,
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DisplayMode mode,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
          log.LogDebug("GetProductionDataTile: " + Request.QueryString);
          ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);

          if (!projectId.HasValue)
          {
              var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
              projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
          }
          Filter filter = CompactionSettings.CompactionFilter(
              startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
              GetMachines(assetID, machineName, isJohnDoe));
          var tileResult = GetProductionDataTile(filter, projectId.Value, mode, (ushort) WIDTH, (ushort) HEIGHT,
              GetBoundingBox(BBOX));
          return tileResult;                      
        }


        /// <summary>
        /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
        /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as elevation, compaction, temperature, cut/fill, volumes etc
        /// </summary>
        /// <param name="SERVICE">WMS parameter - value WMS</param>
        /// <param name="VERSION">WMS parameter - value 1.3.0</param>
        /// <param name="REQUEST">WMS parameter - value GetMap</param>
        /// <param name="FORMAT">WMS parameter - value image/png</param>
        /// <param name="TRANSPARENT">WMS parameter - value true</param>
        /// <param name="LAYERS">WMS parameter - value Layers</param>
        /// <param name="CRS">WMS parameter - value EPSG:4326</param>
        /// <param name="STYLES">WMS parameter - value null</param>
        /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
        /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
        /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
        /// <param name="projectId">Legacy project ID</param>
        /// <param name="projectUid">Project UID</param>
        /// <param name="mode">The thematic mode to be rendered; elevation, compaction, temperature etc</param>
        /// <param name="startUtc">Start UTC.</param>
        /// <param name="endUtc">End UTC. </param>
        /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
        /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
        /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
        /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
        /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
        ///  to be used as the layer type filter. Layer 3 is then the third layer from the
        /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
        /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
        /// May be null/empty, which indicates no restriction.</param>
        /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
        /// All three parameters must be specified to specify a machine. 
        /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
        /// <param name="machineName">See assetID</param>
        /// <param name="isJohnDoe">See assetIDL</param>
        /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request succeeds. 
        /// If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered 
        /// in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to 
        /// indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 
        /// (number of cells across a subgrid) * 0.34 (default width in meters of a single cell).
        /// </returns>
        /// <executor>TilesExecutor</executor> 
        [ProjectIdVerifier]
        [ProjectUidVerifier]
        [Route("api/v2/compaction/productiondatatiles/png")]
        [HttpGet]
        public FileResult GetProductionDataTileRaw(
            [FromQuery] string SERVICE,
            [FromQuery] string VERSION,
            [FromQuery] string REQUEST,
            [FromQuery] string FORMAT,
            [FromQuery] string TRANSPARENT,
            [FromQuery] string LAYERS,
            [FromQuery] string CRS,
            [FromQuery] string STYLES,
            [FromQuery] int WIDTH,
            [FromQuery] int HEIGHT,
            [FromQuery] string BBOX,
            [FromQuery] long? projectId,
            [FromQuery] Guid? projectUid,
            [FromQuery] DisplayMode mode,
            [FromQuery] DateTime? startUtc,
            [FromQuery] DateTime? endUtc,
            [FromQuery] bool? vibeStateOn,
            [FromQuery] ElevationType? elevationType,
            [FromQuery] int? layerNumber,
            [FromQuery] long? onMachineDesignId,
            [FromQuery] long? assetID,
            [FromQuery] string machineName,
            [FromQuery] bool? isJohnDoe)
        {
            log.LogDebug("GetProductionDataTileRaw: " + Request.QueryString);

            ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
            if (!projectId.HasValue)
            {
                var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
                projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
            }
            Filter filter = CompactionSettings.CompactionFilter(
                startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
                GetMachines(assetID, machineName, isJohnDoe));
            var tileResult = GetProductionDataTile(filter, projectId.Value, mode, (ushort) WIDTH, (ushort) HEIGHT,
                GetBoundingBox(BBOX));
            if (tileResult != null)
            {
                Response.Headers.Add("X-Warning", tileResult.TileOutsideProjectExtents.ToString());
                AddCacheResponseHeaders();
                return new FileStreamResult(new MemoryStream(tileResult.TileData), "image/png");
            }

            throw new ServiceException(HttpStatusCode.NoContent,
                new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                    "Raptor failed to return a tile"));
        }

    /// <summary>
    /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
    /// The tiles for the supplied list of files are overlaid and a single tile returned.
    /// </summary>
    /// <param name="SERVICE">WMS parameter - value WMS</param>
    /// <param name="VERSION">WMS parameter - value 1.3.0</param>
    /// <param name="REQUEST">WMS parameter - value GetMap</param>
    /// <param name="FORMAT">WMS parameter - value image/png</param>
    /// <param name="TRANSPARENT">WMS parameter - value true</param>
    /// <param name="LAYERS">WMS parameter - value Layers</param>
    /// <param name="CRS">WMS parameter - value EPSG:4326</param>
    /// <param name="STYLES">WMS parameter - value null</param>
    /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
    /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUids">A collection of imported file IDs for which to to overlay tiles</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
      [ProjectUidVerifier]
      [Route("api/v2/compaction/lineworktiles")]
      [HttpGet]
      public async Task<TileResult> GetLineworkTile(
        [FromQuery] string SERVICE,
        [FromQuery] string VERSION,
        [FromQuery] string REQUEST,
        [FromQuery] string FORMAT,
        [FromQuery] string TRANSPARENT,
        [FromQuery] string LAYERS,
        [FromQuery] string CRS,
        [FromQuery] string STYLES,
        [FromQuery] int WIDTH,
        [FromQuery] int HEIGHT,
        [FromQuery] string BBOX,
        [FromQuery] Guid projectUid,
        [FromQuery] Guid[] fileUids)
      {
        log.LogDebug("GetLineworkTile: " + Request.QueryString);

        ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
        ValidateTileDimensions(WIDTH, HEIGHT);

        var requiredFiles = await ValidateFileUids(projectUid, fileUids);
        DxfTileRequest request = DxfTileRequest.CreateTileRequest(requiredFiles, GetBoundingBox(BBOX));
        request.Validate();
        var executor = RequestExecutorContainer.Build<DxfTileExecutor>(logger, raptorClient, null);
        var result = await executor.ProcessAsync(request)as TileResult;
        return result;
      }

      /// <summary>
      /// This requests returns raw array of bytes with PNG without any diagnostic information. If it fails refer to the request with disgnostic info.
      /// Supplies tiles of linework for DXF, Alignment and Design surface files imported into a project.
      /// The tiles for the supplied list of files are overlaid and a single tile returned.
      /// </summary>
      /// <param name="SERVICE">WMS parameter - value WMS</param>
      /// <param name="VERSION">WMS parameter - value 1.3.0</param>
      /// <param name="REQUEST">WMS parameter - value GetMap</param>
      /// <param name="FORMAT">WMS parameter - value image/png</param>
      /// <param name="TRANSPARENT">WMS parameter - value true</param>
      /// <param name="LAYERS">WMS parameter - value Layers</param>
      /// <param name="CRS">WMS parameter - value EPSG:4326</param>
      /// <param name="STYLES">WMS parameter - value null</param>
      /// <param name="WIDTH">The width, in pixels, of the image tile to be rendered, usually 256</param>
      /// <param name="HEIGHT">The height, in pixels, of the image tile to be rendered, usually 256</param>
      /// <param name="BBOX">The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</param>
      /// <param name="projectUid">Project UID</param>
      /// <param name="fileUids">A collection of imported file IDs for which to to overlay tiles</param>
      /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
      /// <executor>TilesExecutor</executor> 
      [ProjectUidVerifier]
      [Route("api/v2/compaction/lineworktiles/png")]
      [HttpGet]
      public async Task<FileResult> GetLineworkTileRaw(
        [FromQuery] string SERVICE,
        [FromQuery] string VERSION,
        [FromQuery] string REQUEST,
        [FromQuery] string FORMAT,
        [FromQuery] string TRANSPARENT,
        [FromQuery] string LAYERS,
        [FromQuery] string CRS,
        [FromQuery] string STYLES,
        [FromQuery] int WIDTH,
        [FromQuery] int HEIGHT,
        [FromQuery] string BBOX,
        [FromQuery] Guid projectUid,
        [FromQuery] Guid[] fileUids)
      {
        log.LogDebug("GetLineworkTileRaw: " + Request.QueryString);

        ValidateWmsParameters(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT, LAYERS, CRS, STYLES);
        ValidateTileDimensions(WIDTH, HEIGHT);

        var requiredFiles = await ValidateFileUids(projectUid, fileUids);
        DxfTileRequest request = DxfTileRequest.CreateTileRequest(requiredFiles, GetBoundingBox(BBOX));
        request.Validate();
        var executor = RequestExecutorContainer.Build<DxfTileExecutor>(logger, raptorClient, null);
        var result = await executor.ProcessAsync(request) as TileResult;
        if (result != null && result.TileData != null)
        {
          AddCacheResponseHeaders();
          return new FileStreamResult(new MemoryStream(result.TileData), "image/png");
        }

        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "No tile found"));
      }

    /// <summary>
    /// Validates the WMS parameters for the tile requests
    /// </summary>
    /// <param name="SERVICE"></param>
    /// <param name="VERSION"></param>
    /// <param name="REQUEST"></param>
    /// <param name="FORMAT"></param>
    /// <param name="TRANSPARENT"></param>
    /// <param name="LAYERS"></param>
    /// <param name="CRS"></param>
    /// <param name="STYLES"></param>
    private void ValidateWmsParameters(
        string SERVICE,
        string VERSION,
        string REQUEST,
        string FORMAT,
        string TRANSPARENT,
        string LAYERS,
        string CRS,
        string STYLES)
    {
      bool invalid = (!string.IsNullOrEmpty(SERVICE) && SERVICE.ToUpper() != "WMS") ||
                     (!string.IsNullOrEmpty(VERSION) && VERSION.ToUpper() != "1.3.0") ||
                     (!string.IsNullOrEmpty(REQUEST) && REQUEST.ToUpper() != "GETMAP") ||
                     (!string.IsNullOrEmpty(FORMAT) && FORMAT.ToUpper() != "IMAGE/PNG") ||
                     (!string.IsNullOrEmpty(TRANSPARENT) && TRANSPARENT.ToUpper() != "TRUE") ||
                     (!string.IsNullOrEmpty(LAYERS) && LAYERS.ToUpper() != "LAYERS") ||
                     (!string.IsNullOrEmpty(CRS) && CRS.ToUpper() != "EPSG:4326") ||
                     (!string.IsNullOrEmpty(STYLES));
       
        if (invalid)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Service supports only the following: SERVICE=WMS, VERSION=1.3.0, REQUEST=GetMap, FORMAT=image/png, TRANSPARENT=true, LAYERS=Layers, CRS=EPSG:4326, STYLES= (no styles supported)"));
        }
      }

      /// <summary>
      /// Validates the tile width and height
      /// </summary>
      /// <param name="WIDTH"></param>
      /// <param name="HEIGHT"></param>
      private void ValidateTileDimensions(int WIDTH, int HEIGHT)
      {
        if (WIDTH != WebMercatorProjection.TILE_SIZE || HEIGHT != WebMercatorProjection.TILE_SIZE)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Service supports only tile width and height of " + WebMercatorProjection.TILE_SIZE + " pixels"));
        }
      }

      /// <summary>
      /// Validates the file UIDs for DXF tile request and gets the imported file data for them
      /// </summary>
      /// <param name="projectUid">The project UID where the files were imported</param>
      /// <param name="fileUids">The file UIDs of the imported files</param>
      /// <returns>The imported file data for the requested files</returns>
      private async Task<List<FileData>> ValidateFileUids(Guid projectUid, Guid[] fileUids)
      {
        //Check at least one file specified to get tiles for
        if (fileUids == null || fileUids.Length == 0)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "No files selected"));
        }
        //Get all the imported files for the project
        var fileList = await fileListProxy.GetFiles(projectUid.ToString(), Request.Headers.GetCustomHeaders());
        if (fileList == null || fileList.Count == 0)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "No imported files"));
        }
        //Select the required ones from the list
        var fileUidList = fileUids.Select(f => f.ToString()).ToList();
        return fileList.Where(f => fileUidList.Contains(f.ImportedFileUid)).ToList();
      }

      /// <summary>
      /// Adds caching headers to the http response
      /// </summary>
      private void AddCacheResponseHeaders()
      {
        if (!Response.Headers.ContainsKey("Cache-Control"))
        {
          Response.Headers.Add("Cache-Control", "public");
        }
        Response.Headers.Add("Expires",
          DateTime.Now.AddMinutes(15).ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
      }

        /// <summary>
        /// Gets the list of contributing machines from the query parameters
        /// </summary>
        /// <param name="assetID">The asset ID</param>
        /// <param name="machineName">The machine name</param>
        /// <param name="isJohnDoe">The john doe flag</param>
        /// <returns>List of machines</returns>
        private List<MachineDetails> GetMachines(long? assetID, string machineName, bool? isJohnDoe)
        {
            MachineDetails machine = null;
            if (assetID.HasValue || !string.IsNullOrEmpty(machineName) || isJohnDoe.HasValue)
            {
                if (!assetID.HasValue || string.IsNullOrEmpty(machineName) || !isJohnDoe.HasValue)
                {
                    throw new ServiceException(HttpStatusCode.BadRequest,
                        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                            "If using a machine, asset ID machine name and john doe flag must be provided"));
                }
                machine = MachineDetails.CreateMachineDetails(assetID.Value, machineName, isJohnDoe.Value);
            }
            return machine == null ? null : new List<MachineDetails> {machine};
        }

        /// <summary>
        /// Get the bounding box values from the query parameter
        /// </summary>
        /// <param name="bbox">The query parameter containing the bounding box in decimal degrees</param>
        /// <returns>Bounding box in radians</returns>
        private BoundingBox2DLatLon GetBoundingBox(string bbox)
        {
            double blLong = 0;
            double blLat = 0;
            double trLong = 0;
            double trLat = 0;

            int count = 0;
            foreach (string s in bbox.Split(','))
            {
                double num;

                if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
                {
                    throw new ServiceException(HttpStatusCode.BadRequest,
                        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                            "Invalid bounding box"));
                }
                num = num * Math.PI / 180.0; //convert decimal degrees to radians
                //Latitude Must be in range -pi/2 to pi/2 and longitude in the range -pi to pi
                if (count == 0 || count == 2)
                {
                    if (num < -Math.PI / 2)
                    {
                        num = num + Math.PI;
                    }
                    else if (num > Math.PI / 2)
                    {
                        num = num - Math.PI;
                    }
                }
                if (count == 1 || count == 3)
                {
                    if (num < -Math.PI)
                    {
                        num = num + 2 * Math.PI;
                    }
                    else if (num > Math.PI)
                    {
                        num = num - 2 * Math.PI;
                    }
                }

                switch (count++)
                {
                    case 0:
                        blLat = num;
                        break;
                    case 1:
                        blLong = num;
                        break;
                    case 2:
                        trLat = num;
                        break;
                    case 3:
                        trLong = num;
                        break;
                }
            }
            log.LogDebug("BBOX in radians: blLong=" + blLong + ",blLat=" + blLat + ",trLong=" + trLong + ",trLat=" +
                         trLat);
            return BoundingBox2DLatLon.CreateBoundingBox2DLatLon(blLong, blLat, trLong, trLat);
        }

 

    /// <summary>
    /// Gets the requested tile from Raptor
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="projectId"></param>
    /// <param name="mode"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="bbox"></param>
    /// <returns>Tile result</returns>
    private TileResult GetProductionDataTile(Filter filter, long projectId, DisplayMode mode, ushort width, ushort height,
            BoundingBox2DLatLon bbox)
        {
            LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings;
            filter?.Validate();
            ElevationStatisticsResult elevExtents =
                mode == DisplayMode.Height ? GetElevationRange(projectId, filter) : null;
            //Fix bug in Raptor - swap elevations if required
            elevExtents?.SwapElevationsIfRequired();
            TileRequest tileRequest = TileRequest.CreateTileRequest(projectId, null, mode,
                CompactionSettings.CompactionPalette(mode, elevExtents),
                liftSettings, RaptorConverters.VolumesType.None, 0, null, filter, 0, null, 0,
                filter == null ? FilterLayerMethod.None : filter.layerType.Value,
                bbox, null, width, height, 0, CMV_DETAILS_NUMBER_OF_COLORS, false);
            tileRequest.Validate();
            var tileResult = RequestExecutorContainer.Build<TilesExecutor>(logger, raptorClient, null)
                .Process(tileRequest) as TileResult;
            return tileResult;
        }

    
    #endregion

    private const int CMV_DETAILS_NUMBER_OF_COLORS = 11;
  }
}

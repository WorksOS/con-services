using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy to access the TRex Gateway WebAPIs.
  /// </summary>
  public class TRexCompactionDataProxy : BaseProxy, ITRexCompactionDataProxy
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="configurationStore"></param>
    /// <param name="logger"></param>
    public TRexCompactionDataProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    {
    }

    /// <summary>
    /// Sends a request to get CMV % Change statistics from the TRex database.
    /// </summary>
    /// <param name="cmvChangeDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<CMVChangeSummaryResult> SendCMVChangeDetailsRequest(CMVChangeDetailsRequest cmvChangeDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(cmvChangeDetailsRequest);

      log.LogDebug($"{nameof(SendCMVChangeDetailsRequest)}: Sending the request: {request}");

      return SendRequestPost<CMVChangeSummaryResult>(request, customHeaders, "/cmv/percentchange");
    }

    /// <summary>
    /// Sends a request to get CMV Details statistics from the TRex database.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<CMVDetailedResult> SendCMVDetailsRequest(CMVDetailsRequest cmvDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(cmvDetailsRequest);

      log.LogDebug($"{nameof(SendCMVDetailsRequest)}: Sending the request: {request}");

      return SendRequestPost<CMVDetailedResult>(request, customHeaders, "/cmv/details");
    }

    /// <summary>
    /// Sends a request to get CMV Summary statistics from the TRex database.
    /// </summary>
    /// <param name="cmvSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<CMVSummaryResult> SendCMVSummaryRequest(CMVSummaryRequest cmvSummaryRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(cmvSummaryRequest);

      log.LogDebug($"{nameof(SendCMVSummaryRequest)}: Sending the request: {request}");

      return SendRequestPost<CMVSummaryResult>(request, customHeaders, "/cmv/summary");
    }

    /// <summary>
    /// Sends a request to get Pass Count Details statistics from the TRex database.
    /// </summary>
    /// <param name="pcDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<PassCountDetailedResult> SendPassCountDetailsRequest(PassCountDetailsRequest pcDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(pcDetailsRequest);

      log.LogDebug($"{nameof(SendPassCountDetailsRequest)}: Sending the request: {request}");

      return SendRequestPost<PassCountDetailedResult>(request, customHeaders, "/passcounts/details");
    }

    /// <summary>
    /// Sends a request to get Pass Count Summary statistics from the TRex database.
    /// </summary>
    /// <param name="pcSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<PassCountSummaryResult> SendPassCountSummaryRequest(PassCountSummaryRequest pcSummaryRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(pcSummaryRequest);

      log.LogDebug($"{nameof(SendPassCountSummaryRequest)}: Sending the request: {request}");

      return SendRequestPost<PassCountSummaryResult>(request, customHeaders, "/passcounts/summary");
    }

    /// <summary>
    /// Sends a request to get Cut/Fill Details statistics from the TRex database.
    /// </summary>
    /// <param name="cfDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<CompactionCutFillDetailedResult> SendCutFillDetailsRequest(CutFillDetailsRequest cfDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(cfDetailsRequest);

      log.LogDebug($"{nameof(SendCutFillDetailsRequest)}: Sending the request: {request}");

      return SendRequestPost<CompactionCutFillDetailedResult>(request, customHeaders, "/cutfill/details");
    }

    /// <summary>
    /// Sends a request to get MDP Summary statistics from the TRex database.
    /// </summary>
    /// <param name="mdpSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<MDPSummaryResult> SendMDPSummaryRequest(MDPSummaryRequest mdpSummaryRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(mdpSummaryRequest);

      log.LogDebug($"{nameof(SendMDPSummaryRequest)}: Sending the request: {request}");

      return SendRequestPost<MDPSummaryResult>(request, customHeaders, "/mdp/summary");
    }

    /// <summary>
    /// Sends a request to get Material Temperature Summary statistics from the TRex database.
    /// </summary>
    /// <param name="temperatureSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<TemperatureSummaryResult> SendTemperatureSummaryRequest(TemperatureSummaryRequest temperatureSummaryRequest,
      IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(temperatureSummaryRequest);

      log.LogDebug($"{nameof(SendTemperatureSummaryRequest)}: Sending the request: {request}");

      return SendRequestPost<TemperatureSummaryResult>(request, customHeaders, "/temperature/summary");
    }

    public Task<TemperatureDetailResult> SendTemperatureDetailsRequest(TemperatureDetailRequest temperatureDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(temperatureDetailsRequest);

      log.LogDebug($"{nameof(SendTemperatureDetailsRequest)}: Sending the request: {request}");

      return SendRequestPost<TemperatureDetailResult>(request, customHeaders, "/temperature/details");
    }

    /// <summary>
    /// Sends a request to get Machine Speed Summary statistics from the TRex database.
    /// </summary>
    /// <param name="speedSummaryRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<SpeedSummaryResult> SendSpeedSummaryRequest(SpeedSummaryRequest speedSummaryRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(speedSummaryRequest);

      log.LogDebug($"{nameof(SendSpeedSummaryRequest)}: Sending the request: {request}");

      return SendRequestPost<SpeedSummaryResult>(request, customHeaders, "/speed/summary");
    }

    /// <summary>
    /// Sends a request to get production data tile from the TRex database.
    /// </summary>
    /// <param name="tileRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<Stream> SendProductionDataTileRequest(TileRequest tileRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(tileRequest);

      log.LogDebug($"{nameof(SendProductionDataTileRequest)}: Sending the request: {request}");
      
      return SendRequestPostAsStreamContent(request, customHeaders, "/tile");
    }

    /// <summary>
    /// Sends a request to get Summary Volumes statistics from the TRex database.
    /// </summary>
    /// <param name="summaryVolumesRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<SummaryVolumesResult> SendSummaryVolumesRequest(SummaryVolumesDataRequest summaryVolumesRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(summaryVolumesRequest);

      log.LogDebug($"{nameof(SendSummaryVolumesRequest)}: Sending the request: {request}");

      return SendRequestPost<SummaryVolumesResult>(request, customHeaders, "/volumes/summary");
    }

    /// <summary>
    /// Sends a request to get project extents for a site model from the TRex database.
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<BoundingBox3DGrid> SendProjectExtentsRequest(string siteModelID, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendProjectExtentsRequest)}: Sending the get project extents request for site model ID: {siteModelID}");

      return SendRequestGet<BoundingBox3DGrid>(customHeaders, $"/sitemodels/{siteModelID}/extents");
    }

    /// <summary>
    /// Sends a request to get a TIN surface data from the TRex database.
    /// </summary>
    /// <param name="compactionExportRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<CompactionExportResult> SendSurfaceExportRequest(CompactionExportRequest compactionExportRequest,
      IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(compactionExportRequest);

      log.LogDebug($"{nameof(SendSurfaceExportRequest)}: Sending the request: {request}");

      return SendRequestPost<CompactionExportResult>(request, customHeaders, "/export/surface/ttm");
    }

    /// <summary>
    /// Sends a request to get production data patches from the TRex database.
    /// </summary>
    /// <param name="patchDataRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public Task<Stream> SendProductionDataPatchRequest(PatchDataRequest patchDataRequest, IDictionary<string, string> customHeaders = null)
    {
      var request = JsonConvert.SerializeObject(patchDataRequest);

      log.LogDebug($"{nameof(SendProductionDataPatchRequest)}: Sending the request: {request}");

      return SendRequestPostAsStreamContent(request, customHeaders, "/patches");
    }




    /// <summary>
    /// Executes a POST request against the TRex Gateway service.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    private Task<T> SendRequestPost<T>(string payload, IDictionary<string, string> customHeaders, string route) where T : ContractExecutionResult
    {
      var response = SendRequest<T>("TREX_GATEWAY_API_URL", payload, customHeaders, route, HttpMethod.Post, string.Empty);

      log.LogDebug($"{nameof(SendRequestPost)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    /// <summary>
    /// Executes a POST request against the TRex Gateway service.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    private Task<T> SendRequestPostEx<T>(string payload, IDictionary<string, string> customHeaders, string route) where T : ActionResult
    {
      var response = SendRequest<T>("TREX_GATEWAY_API_URL", payload, customHeaders, route, HttpMethod.Post, string.Empty);

      log.LogDebug($"{nameof(SendRequestPostEx)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }

    /// <summary>
    /// Executes a POST request against the TRex Gateway service as stream content.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="contentType"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    private async Task<Stream> SendRequestPostAsStreamContent(string payload, IDictionary<string, string> customHeaders, string route)
    {
      var result = await  GetMasterDataStreamContent("TREX_GATEWAY_API_URL", customHeaders, "POST", payload, null, route);

      return result;
    }

    /// <summary>
    /// Executes a GET request against the TRex Gateway service.
    /// </summary>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    private Task<T> SendRequestGet<T>(IDictionary<string, string> customHeaders, string route, string queryParameters = null)
    {
      var response = SendRequest<T>("TREX_GATEWAY_API_URL", string.Empty, customHeaders, route, HttpMethod.Get, queryParameters);

      log.LogDebug($"{nameof(SendRequestGet)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}

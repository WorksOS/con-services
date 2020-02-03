using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace CCSS.TagFileSplitter.WebAPI.Common.Helpers
{
  public static class TargetServiceHelper
  {
    /// <summary>
    /// Sends tag file to a 3dpm endpoint, retrieving result
    ///   With CCSS we may not be able to use serviceDiscovery as it'll be in different cluster
    ///   This could be extended to use specific urls, not known to our service discovery
    /// </summary>
    /// <returns></returns>
    public static async Task<TargetServiceResponse> SendTagFileTo3dPmService(CompactionTagFileRequest compactionTagFileRequest,
      IServiceResolution serviceResolution, IGenericHttpProxy genericHttpProxy,
      string serviceName, ApiVersion apiVersion, string route,
      ILogger log, IDictionary<string, string> customHeaders, int? timeout = null)
    {
      log.LogDebug($"{nameof(SendTagFileTo3dPmService)}  serviceName: {serviceName} apiVersion: {apiVersion} route: {route}");
      var targetServiceResponse = new TargetServiceResponse(serviceName, (int)TAGProcServerProcessResultCode.Unknown);

      try
      {
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(compactionTagFileRequest))))
        {
          //var service = await serviceResolution.ResolveService(serviceName); // find in configStore/kubernetes etc
          //if (string.IsNullOrEmpty(service.Endpoint))
          //  throw new ServiceException(HttpStatusCode.InternalServerError,
          //  new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          //    $"{nameof(SendTagFileTo3dPmService)}: Unable to resolve target service url. serviceName: {serviceName}"));

          var url = await serviceResolution.ResolveRemoteServiceEndpoint(serviceName, ApiType.Public, apiVersion, route);
          if (string.IsNullOrEmpty(url))
            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                $"{nameof(SendTagFileTo3dPmService)}: Unable to resolve target service endpoint: {serviceName}"));

          targetServiceResponse = await genericHttpProxy.ExecuteGenericHttpRequest<TargetServiceResponse>(url, HttpMethod.Post, ms, customHeaders, timeout);
          
          // this is how the statusCode is set in 3dpm controller
          targetServiceResponse.StatusCode = targetServiceResponse.Code == 0
            ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
          targetServiceResponse.ServiceName = serviceName; 
        }
      }
      catch (ServiceException se)
      {
        // these could come from the above request BEFORE or AFTER sent to 3dp e.g. ?
        log.LogError(se, $"{nameof(SendTagFileTo3dPmService)}: returned service exception");
        targetServiceResponse = new TargetServiceResponse(serviceName, se.GetResult.Code, se.GetResult.Message, se.Code); 
      }
      catch (HttpRequestException re)
      {
        // Polly (in this TFS service) calls 3dp. If 3dp throws a ServiceException, Polly code converts it to a HttpRequestException
        log.LogWarning(re, $"{nameof(SendTagFileTo3dPmService)}: returned request service exception");
        var parts = re.Message.Split(new[]{" {", "}" }, StringSplitOptions.None);
        if (parts.Length == 3)
        {
          var isParsedOk = Enum.TryParse<HttpStatusCode>(parts[0], out var statusCode);
          if (!isParsedOk)
            statusCode = HttpStatusCode.BadRequest;
          var contractException = JsonConvert.DeserializeObject<ContractExecutionResult>('{' + parts[1] + '}');
          log.LogWarning($"{nameof(SendTagFileTo3dPmService)} Exception: statusCode: {statusCode} code: {contractException.Code} message: {contractException.Message}");
          targetServiceResponse = new TargetServiceResponse(serviceName, contractException.Code, contractException.Message, statusCode);
        }
        else
          targetServiceResponse = new TargetServiceResponse(serviceName, (int)TAGProcServerProcessResultCode.Unknown, re.Message, HttpStatusCode.InternalServerError);
      }
      catch (Exception e)
      {
        // could this come from the above request BEFORE or AFTER sent to 3dp e.g. ArgumentException from BaseServiceProxy
        log.LogError(e, $"{nameof(SendTagFileTo3dPmService)}: returned exception");
        targetServiceResponse = new TargetServiceResponse(serviceName, (int)TAGProcServerProcessResultCode.Unknown, e.Message, HttpStatusCode.InternalServerError);
      }

      return targetServiceResponse;
    }
  }
}

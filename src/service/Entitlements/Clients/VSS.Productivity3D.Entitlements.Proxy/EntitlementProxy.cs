using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;

namespace VSS.Productivity3D.Entitlements.Proxy
{
    public class EntitlementProxy : BaseServiceDiscoveryProxy, IEntitlementProxy
    {
      private const string ENABLE_ENTITLEMENTS_CONFIG_KEY = "ENABLE_ENTITLEMENTS_CHECKING";

      public EntitlementProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
      {

      }

      public override bool IsInsideAuthBoundary => true;

      public override ApiService InternalServiceType => ApiService.Entitlements;

      public override string ExternalServiceName => null;

      public override ApiVersion Version => ApiVersion.V1;

      public override ApiType Type => ApiType.Public;

      public override string CacheLifeKey => "ENTITLEMENT_CACHE_LIFE";

      /// <summary>
      /// Attempt to fetch the entitlement response from the entitlement server
      /// Can return a null response in the event of a bad request / mismatched user email to email attached to bearer token
      /// </summary>
      /// <returns>Model indicated if the entitlement is allowed, or not - or null in the event of a bad request</returns>
      public async Task<EntitlementResponseModel> IsEntitled(EntitlementRequestModel request, IHeaderDictionary customHeaders = null)
      {
        if(request == null)
          throw new ArgumentException("No request provided", nameof(request));

        // In some cases we want to disable entitlements checking, e.g tests or staging 
        // this key allows that at a global level to be disabled, but calling code still operates the same
        if (!configurationStore.GetValueBool(ENABLE_ENTITLEMENTS_CONFIG_KEY, false))
        {
          log.LogInformation($"Entitlements checking is disabled for request {JsonConvert.SerializeObject(request)}");
          return  new EntitlementResponseModel() 
          {
            IsEntitled = true, 
            Feature = request.Feature, 
            OrganizationIdentifier = request.OrganizationIdentifier, 
            UserEmail = request.UserEmail
          };
        }

        try
        {
          await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));
          var result = await PostMasterDataItemServiceDiscovery<EntitlementResponseModel>("/entitlement", request.OrganizationIdentifier, request.UserEmail, customHeaders, null, ms);
          if (result == null)
          {
            log.LogInformation($"No Entitlement returned from the Entitlement Service, returned a failed entitlement request to the caller.");
            return new EntitlementResponseModel() 
            {
              IsEntitled = false, 
              Feature = request.Feature, 
              OrganizationIdentifier = request.OrganizationIdentifier, 
              UserEmail = request.UserEmail
            };
          }
          log.LogInformation($"User `{result.UserEmail}` for Customer: `{request.OrganizationIdentifier}` {(result.IsEntitled ? "is" : "is not")} entitled to use the `{request.Feature}` feature.");
          return result;
        }
        catch (HttpRequestException e)
        {
          log.LogWarning($"Failed to get entitlement, got exception with message: {e.Message}");
          return null;
        }
      }
    }
}

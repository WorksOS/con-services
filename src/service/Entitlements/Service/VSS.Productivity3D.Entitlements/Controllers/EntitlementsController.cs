using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Entitlements.WebApi.Controllers
{
  /// <summary>
  /// API end points for entitlement checks.
  /// </summary>
  public class EntitlementsController : Controller
  {
    private ILogger<EntitlementsController> _logger;
    private IConfigurationStore _configurationStore;
    private ITPaaSApplicationAuthentication _authn;
    private IEmsClient _emsClient;
    //private readonly bool _enableEntitlementCheck;

    /// <summary> Gets the application logging interface. </summary>
    private ILogger<EntitlementsController> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<EntitlementsController>>();

    /// <summary> Gets the configuration store </summary>
    private IConfigurationStore ConfigStore => _configurationStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();

    /// <summary> The TPaaS authentication for generating an application bearer token </summary>
    private ITPaaSApplicationAuthentication Authn => _authn ??= HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>();

    /// <summary> The EMS (entitlement management system) client </summary>
    private IEmsClient EmsClient => _emsClient ??= HttpContext.RequestServices.GetService<IEmsClient>();

    /// <summary> Constructor </summary>
    public EntitlementsController()
    {
      //_enableEntitlementCheck = ConfigStore.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false);
    }

    /// <summary>
    /// Attempt to request an entitlement for the request feature and user
    /// </summary>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EntitlementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost("api/v1/entitlement")]
    public async Task<IActionResult> GetEntitlement([FromBody] EntitlementRequestModel request)
    {
      Logger.LogInformation($"Entitlement Request: {JsonConvert.SerializeObject(request)}");

      if (request == null)
        return BadRequest();

      var validationResult = request.Validate(User.Identity.Name);
      if (validationResult.Code == ContractExecutionStatesEnum.ValidationError)
      {
        Logger.LogWarning(validationResult.Message);
        return BadRequest(validationResult.Message);
      }

      var enableEntitlementCheck = ConfigStore.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false);

      var isEntitled = false;
      if (enableEntitlementCheck)
      {
        var statusCode = await EmsClient.GetEntitlements(Guid.Parse(request.UserUid), Guid.Parse(request.OrganizationIdentifier), request.Sku, request.Feature, CustomHeaders);
        isEntitled = statusCode == HttpStatusCode.Accepted;
      }
      else
      {
        Logger.LogInformation($"Entitlement checking is disabled, allowing the request.");
        isEntitled = true;
      }

      //TODO: Should we return Forbid(); for HttpStatusCode.NoContent as per #502 ?
      var response = new EntitlementResponseModel
        {
          Feature = request.Feature,
          Sku = request.Sku,
          IsEntitled = isEntitled,
          OrganizationIdentifier = request.OrganizationIdentifier,
          UserUid = request.UserUid
        };

      Logger.LogInformation($"Generated Entitlements Response: {JsonConvert.SerializeObject(response)}");

      return Json(response);
    }

    private IHeaderDictionary CustomHeaders =>
      new HeaderDictionary
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {Authn.GetApplicationBearerToken()}"}
      };
  }
}

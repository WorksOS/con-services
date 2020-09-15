using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary> Gets the application logging interface. </summary>
    private ILogger<EntitlementsController> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<EntitlementsController>>();

    /// <summary> Gets the configuration store </summary>
    private IConfigurationStore ConfigStore => _configurationStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();

    /// <summary> The TPaaS authentication for generating an application bearer token </summary>
    private ITPaaSApplicationAuthentication Authn => _authn ??= HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>();

    /// <summary> The EMS (entitlement management system) client </summary>
    private IEmsClient EmsClient => _emsClient ??= HttpContext.RequestServices.GetService<IEmsClient>();

    /// <summary> List of users who automatically have entitlement e.g. Team Merino users </summary>
    private static List<string> AcceptedEmails;

    private static string WorksOsFeature;
    private static string WorksOsSku;

    /// <summary> Constructor </summary>
    public EntitlementsController()
    {
     
    }

    /// <summary>
    /// Attempt to request an entitlement for the request feature and user. Used by WorksOS services internally.
    /// </summary>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost("internal/v1/entitlement")]
    public async Task<IActionResult> GetEntitlementInternal([FromBody] EntitlementRequestModel request)
    {
      Logger.LogInformation($"Internal Entitlement Request: {JsonConvert.SerializeObject(request)}");

      if (request == null)
        return BadRequest();

      var validationResult = request.Validate(User.Identity.Name);
      if (validationResult.Code == ContractExecutionStatesEnum.ValidationError)
      {
        Logger.LogWarning(validationResult.Message);
        return BadRequest(validationResult.Message);
      }

      var isEntitled = false;
      if (!string.IsNullOrEmpty(request.UserEmail))
      {
        if (AcceptedEmails == null)
        {
          Logger.LogInformation($"Loading testing entitlement accepted emails");
          LoadTestingEmails();
        }
        isEntitled = AcceptedEmails.Contains(request.UserEmail.ToLower());
        _logger.LogInformation($"{request.UserEmail} {(isEntitled ? "is an accepted email" : "is not in the allowed list")}");
      }

      if (!isEntitled)
      {
        var enableEntitlementCheck = ConfigStore.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false);
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
      }

      var response = new EntitlementResponseModel
        {
          Feature = request.Feature,
          Sku = request.Sku,
          IsEntitled = isEntitled,
          OrganizationIdentifier = request.OrganizationIdentifier,
          UserUid = request.UserUid,
          UserEmail = request.UserEmail
        };

      Logger.LogInformation($"Generated Entitlements Response: {JsonConvert.SerializeObject(response)}");

      return Json(response);
    }

    /// <summary>
    /// Attempt to request an entitlement for the request feature and user. Used by WorksOS UI.
    /// </summary>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost("api/v1/entitlement")]
    public async Task<IActionResult> GetEntitlementExternal([FromBody] ExternalEntitlementRequestModel request)
    {
      Logger.LogInformation($"External Entitlement Request: {JsonConvert.SerializeObject(request)}");

      if (request == null)
        return BadRequest();

      LoadFeaturesAndSkus();
      (var feature, var sku) = MapApplicationToFeature(request.ApplicationName);
      if (string.IsNullOrEmpty(feature) || string.IsNullOrEmpty(sku))
        return BadRequest($"Unknown application {request.ApplicationName}");

      return await GetEntitlementInternal(new EntitlementRequestModel
      {
        OrganizationIdentifier = request.OrganizationIdentifier,
        Feature = feature,
        Sku = sku,
        UserEmail = request.UserEmail,
        UserUid = User.Identity.Name
      });
    }

    /// <summary>
    /// Loads the configured features and skus. Currently ony WorksOS.
    /// </summary>
    private void LoadFeaturesAndSkus()
    {
      if (string.IsNullOrEmpty(WorksOsFeature))
        WorksOsFeature = ConfigStore.GetValueString(ConfigConstants.ENTITLEMENTS_FEATURE_CONFIG_KEY, "FEA-CEC-WORKSOS");
      if (string.IsNullOrEmpty(WorksOsSku))
        WorksOsSku = ConfigStore.GetValueString(ConfigConstants.ENTITLEMENTS_SKU_CONFIG_KEY, "HCC-WOS-MO");
    }

    /// <summary>
    /// Maps the application to the feature and sku for EMS.
    /// </summary>
    private (string feature, string sku) MapApplicationToFeature(string appName) => appName switch
    {
      "worksos" => (WorksOsFeature, WorksOsSku),
      _ => (string.Empty, string.Empty)
    };

    private IHeaderDictionary CustomHeaders =>
      new HeaderDictionary
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {Authn.GetApplicationBearerToken()}"}
      };

    /// <summary>
    /// A list of hardcoded emails of users, primarily developers, who automatically have entitlement to use WorksOS.
    /// This is to bypass calling EMS and to avoid having to set up licenses for them.
    /// These will be loaded from env, once.
    /// </summary>
    private void LoadTestingEmails()
    {
      var data = ConfigStore.GetValueString(ConfigConstants.ENTITLEMENTS_ACCEPT_EMAIL_KEY, string.Empty);
      if (string.IsNullOrEmpty(data))
      {
        Logger.LogWarning($"No Allowed Emails for Entitlements loaded");
        AcceptedEmails = new List<string>();
      }
      else
      {
        AcceptedEmails = data.Split(';').Select(e => e.ToLower().Trim()).ToList();
        foreach (var email in AcceptedEmails)
        {
          Logger.LogInformation($"Accepting entitlements from `{email}`");
        }
      }
    }
  }
}

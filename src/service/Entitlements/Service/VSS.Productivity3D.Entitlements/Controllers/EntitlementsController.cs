using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.Productivity3D.Entitlements.Authentication;

namespace VSS.Productivity3D.Entitlements.Controllers
{
  public class EntitlementsController : Controller
  {
    private const string ENTITLEMENTS_ACCEPT_EMAIL_KEY = "ENTITLEMENTS_ALLOWED_EMAILS";
    private readonly IConfigurationStore _configurationStore;
    private readonly ILogger<EntitlementsController> _logger;
    private static List<string> AcceptedEmails;

    public EntitlementsController(IConfigurationStore configurationStore,ILogger<EntitlementsController> logger)
    {
      _configurationStore = configurationStore;
      _logger = logger;
      if (AcceptedEmails == null)
      {
        _logger.LogInformation($"Loading testing entitlement accepted emails");
        LoadTestingEmails();
      }
    }

    public EntitlementUserClaim User
    {
      get
      {
        return HttpContext?.User as EntitlementUserClaim;
      }
    }

    /// <summary>
    /// Attempt to request an entitlement for the request feature and user
    /// </summary>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EntitlementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost("api/v1/entitlement")]
    public IActionResult GetEntitlement([FromBody] EntitlementRequestModel request)
    {
      if (request == null)
        return BadRequest();

      var user = User;
      if (user == null || string.IsNullOrEmpty(user.UserEmail))
      {
        var message = $"{(user == null ? "User is null" : "User email is empty.")}";
        _logger.LogWarning(message);
        return BadRequest(message);
      }

      if (string.Compare(user.UserEmail, request.UserEmail, StringComparison.InvariantCultureIgnoreCase) != 0)
      {
        _logger.LogWarning($"Provided email {request.UserEmail} does not match JWT TID email: {user.UserEmail}");
        return BadRequest($"Provided email does not match JWT");
      }

      if (string.IsNullOrEmpty(request.OrganizationIdentifier))
      {
        _logger.LogWarning("No Organization Identifier provided");
        return BadRequest("No Organization Identifier provided");
      }

      _logger.LogInformation($"Entitlement Request: {JsonConvert.SerializeObject(request)}");

      // Temporary until we have an external endpoint
      if(string.Compare(request.Feature, "worksos", StringComparison.InvariantCultureIgnoreCase) != 0)
        return StatusCode((int) HttpStatusCode.Forbidden);

      var isEmailAccepted = AcceptedEmails.Contains(request.UserEmail.ToLower());
      _logger.LogInformation($"{request.UserEmail} {(isEmailAccepted ? "is an accepted email" : "is not in the allowed list")}");

      var response = new EntitlementResponseModel
      {
        Feature = request.Feature, 
        IsEntitled = isEmailAccepted, 
        OrganizationIdentifier = request.OrganizationIdentifier, 
        UserEmail = request.UserEmail
      };

      _logger.LogInformation($"Generated Entitlements Response: {JsonConvert.SerializeObject(response)}");

      return Json(response);
    }

    /// <summary>
    /// While we wait for a real entitlement server, we will use a list of hardcoded emails
    /// These will be loaded from env, once.
    /// </summary>
    private void LoadTestingEmails()
    {
      var data = _configurationStore.GetValueString(ENTITLEMENTS_ACCEPT_EMAIL_KEY, string.Empty);
      if (string.IsNullOrEmpty(data))
      {
        _logger.LogWarning($"No Allowed Emails for Entitlements loaded");
        AcceptedEmails = new List<string>();
      }

        
      AcceptedEmails = data.Split(';').Select(e => e.ToLower().Trim()).ToList();
      foreach (var email in AcceptedEmails)
      {
        _logger.LogInformation($"Accepting entitlements from `{email}`");
      }
    }
  }
}

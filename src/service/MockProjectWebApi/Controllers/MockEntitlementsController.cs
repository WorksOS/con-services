using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;

namespace MockProjectWebApi.Controllers
{
  public class MockEntitlementsController : Controller
  {
    protected readonly ILogger Logger;
    protected readonly ILoggerFactory LoggerFactory;


    public MockEntitlementsController(ILoggerFactory loggerFactory)
    {
      LoggerFactory = loggerFactory;
      Logger = loggerFactory.CreateLogger(GetType());
    }

    [HttpPost("api/v1/entitlement")]
    public IActionResult GetMockEntitlement([FromBody] EntitlementRequestModel request)
    {
      Logger.LogInformation($"{nameof(GetMockEntitlement)}: UserUid={request.UserUid}");

      var response = new EntitlementResponseModel
      {
        Feature = request.Feature,
        Sku = request.Sku,
        IsEntitled = true,
        OrganizationIdentifier = request.OrganizationIdentifier,
        UserUid = request.UserUid,
        UserEmail = request.UserEmail
      };
      return Json(response);
    }
  }
}

using System;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VSS.Productivity3D.Entitlements.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.Productivity3D.Entitlements.Common.Authentication;
using VSS.Productivity3D.Entitlements.WebApi.Controllers;
using Xunit;

namespace VSS.Productivity3D.Entitlements.UnitTests
{
  public class EntitlementsControllerTests : UnitTestsDIFixture<EntitlementsControllerTests>
  {
    [Fact]
    public async Task GetEntitlement_Success()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(true);

      mockEmsClient.Setup(e => e.GetEntitlements(userUid, customerUid, request.Sku, request.Feature, It.IsAny<IHeaderDictionary>())).ReturnsAsync(HttpStatusCode.Accepted);

      mockAuthn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlement(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.True(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_NotEntitled()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(true);

      mockEmsClient.Setup(e => e.GetEntitlements(userUid, customerUid, request.Sku, request.Feature, It.IsAny<IHeaderDictionary>())).ReturnsAsync(HttpStatusCode.NoContent);

      mockAuthn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlement(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.False(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_CheckDisabled()
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = customerUid.ToString(),
        UserUid = userUid.ToString(),
        Sku = "some sku",
        Feature = "some feature"
      };

      mockConfigStore.Setup(c => c.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false)).Returns(false);

      var controller = CreateEntitlementsController(userUid.ToString());
      var result = await controller.GetEntitlement(request);
      Assert.NotNull(result);
      var response = (result as JsonResult)?.Value as EntitlementResponseModel;
      Assert.NotNull(response);
      Assert.Equal(request.OrganizationIdentifier, response.OrganizationIdentifier);
      Assert.Equal(request.UserUid, response.UserUid);
      Assert.Equal(request.Sku, response.Sku);
      Assert.Equal(request.Feature, response.Feature);
      Assert.True(response.IsEntitled);
    }

    [Fact]
    public async Task GetEntitlement_NoRequest()
    {
      var controller = CreateEntitlementsController(Guid.NewGuid().ToString());
      var result = await controller.GetEntitlement(null);
      Assert.NotNull(result);
      var response = result as BadRequestResult;
      Assert.NotNull(response);
      Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task GetEntitlement_DifferentUserUid()
    {
      var request = new EntitlementRequestModel
      {
        OrganizationIdentifier = Guid.NewGuid().ToString(),
        UserUid = Guid.NewGuid().ToString(),
        Sku = "some sku",
        Feature = "some feature"
      };

      var controller = CreateEntitlementsController(Guid.NewGuid().ToString());
      var result = await controller.GetEntitlement(request);
      Assert.NotNull(result);
      var response = result as BadRequestObjectResult;
      Assert.NotNull(response);
      Assert.Equal(400, response.StatusCode);
      Assert.Equal("Provided uuid does not match JWT.", response.Value);
    }

    private EntitlementsController CreateEntitlementsController(string userUid)
    {
      var httpContext = new DefaultHttpContext();
      httpContext.RequestServices = ServiceProvider;
      httpContext.User = new EntitlementUserClaim(new GenericIdentity(userUid), null, null, false);
      var controllerContext = new ControllerContext();
      controllerContext.HttpContext = httpContext;
      var controller = new EntitlementsController();
      controller.ControllerContext = controllerContext;
      return controller;
    }
  }
}

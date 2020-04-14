using System;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Common.Models;
using CCSS.Productivity3D.Preferences.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.WebApi.Common;
using Xunit;
using UserPrefKeyDataModel = CCSS.Productivity3D.Preferences.Abstractions.Models.Database.UserPreferenceKey;

namespace CCSS.Productivity3D.Preferences.Tests
{
  public class ControllerValidationTests : UnitTestsDIFixture<ControllerValidationTests>
  {
    [Fact]
    public async Task ValidateUserId_UserIdMatches()
    {
      const string keyName = "some key";
      var userUid = Guid.NewGuid();

      var userPrefDatabase = new UserPrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = Guid.NewGuid().ToString(),
        PreferenceJson = "some json",
        SchemaVersion = "1.0"
      };

      mockPrefRepo.Setup(p => p.GetUserPreference(userUid, keyName))
              .ReturnsAsync(userPrefDatabase);

      var controller = CreatePreferencesController(userUid.ToString(), false);
      var result = await controller.GetUserPreferenceV1(keyName, userUid);
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Equal(userPrefDatabase.KeyName, result.PreferenceKeyName);
      Assert.Equal(userPrefDatabase.PreferenceKeyUID, result.PreferenceKeyUID.ToString());
      Assert.Equal(userPrefDatabase.PreferenceJson, result.PreferenceJson);
      Assert.Equal(userPrefDatabase.SchemaVersion, result.SchemaVersion);
    }

    [Fact]
    public async Task ValidateUserId_UserIdDiffers()
    {
      const string keyName = "some key";
      var userUid1 = Guid.NewGuid();
      var userUid2 = Guid.NewGuid();

      var controller = CreatePreferencesController(userUid1.ToString(), false);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await controller.GetUserPreferenceV1(keyName, userUid2)) as ServiceException;
      Assert.Equal(HttpStatusCode.Forbidden, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2008, result.Code);
      Assert.Equal("Access denied.", result.Message);
    }

    [Fact]
    public async Task ValidateUserId_MissingUserId()
    {
      const string keyName = "some key";
      var applicationId = "some app";
      
      var controller = CreatePreferencesController(applicationId, true);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await controller.GetUserPreferenceV1(keyName)) as ServiceException;
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2009, result.Code);
      Assert.Equal("Missing user UID.", result.Message);
    }

    [Fact]
    public async Task ValidateCreate_ExistingNoUpdate()
    {
      const string keyName = "some key";
      var userUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var userPrefDatabase = new UserPrefKeyDataModel
      {
        KeyName = keyName,
        PreferenceKeyUID = prefKeyUid.ToString(),
        PreferenceJson = "some json",
        SchemaVersion = "1.0"
      };

      mockPrefRepo.Setup(p => p.GetUserPreference(userUid, keyName))
              .ReturnsAsync(userPrefDatabase);

      var request = new UpsertUserPreferenceRequest
      {
        PreferenceJson = "some new json",
        PreferenceKeyUID = prefKeyUid,
        PreferenceKeyName = userPrefDatabase.KeyName,
        SchemaVersion = userPrefDatabase.SchemaVersion,
        TargetUserUID = userUid
      };
      var controller = CreatePreferencesController(userUid.ToString(), false);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await controller.CreateUserPreference(request)) as ServiceException;
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      var result = ex.GetResult;
      Assert.Equal(2013, result.Code);
      Assert.Equal("User preference already exists. ", result.Message);
    }

    private PreferencesController CreatePreferencesController(string userUid, bool isApp)
    {
      var httpContext = new DefaultHttpContext();
      httpContext.RequestServices = ServiceProvider;
      httpContext.User = new TIDCustomPrincipal(new GenericIdentity(userUid), null, null, null, isApp, null);
      var controllerContext = new ControllerContext();
      controllerContext.HttpContext = httpContext;
      var controller = new PreferencesController();
      controller.ControllerContext = controllerContext;
      return controller;
    }
  }
}

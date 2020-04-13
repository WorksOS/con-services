using System;
using System.Net;
using System.Threading.Tasks;
using TestUtility;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace WebApiTests
{
  public class UserPreferenceWebTests : WebApiTestsBase
  {
    [Fact]
    public async Task CreateUserPreferenceHappyPath()
    {
      Msg.Title("User Preference test 1", "Create user preference successfully");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | " ,
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | some json here |"};
      var request = await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);
      await ts.GetUserPreferenceViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerUid, request, false);
    }

    [Fact]
    public async Task CreateUserPreferenceExistingNoUpdate()
    {
      Msg.Title("User Preference test 2", "Create duplicate user preference");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | " ,
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | some json here |" };
      await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);

      var prefEventArray2 = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | different json |"};
      var response2 = await ts.PublishEventToWebApi<ContractExecutionResult>(prefEventArray2, statusCode: HttpStatusCode.BadRequest);
      Assert.True(response2 == $"User preference already exists. ", "Response is unexpected. Should fail with unable to create. Response: " + response2);
    }

    [Fact]
    public async Task CreateUserPreferenceExistingAllowUpdate()
    {
      Msg.Title("User Preference test 3", "Upsert user preference");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | " ,
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | some json here |" };
      await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);

      var prefEventArray2 = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | different json |"};
      var response2 = await ts.PublishEventToWebApi<ContractExecutionResult>(prefEventArray2, "?allowUpdate=true");
      Assert.True(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);
    }

    [Fact]
    public async Task UpdateUserPreferenceHappyPath()
    {
      Msg.Title("User Preference test 4", "Update user preference successfully");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | " ,
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | some json here |",
      $"| UpdateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | different json |"};
      var request = await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);
      await ts.GetUserPreferenceViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerUid, request, false);
    }

    [Fact]
    public async Task UpdateUserPreferenceNoExisting()
    {
      Msg.Title("User Preference test 5", "Update non-existant user preference");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | "};
      await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);

      var prefEventArray2 = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| UpdateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | some json here |"};
      var response2 = await ts.PublishEventToWebApi<ContractExecutionResult>(prefEventArray2, statusCode: HttpStatusCode.InternalServerError);
      Assert.True(response2 == $"Unable to update user preference. ", "Response is unexpected. Should fail with unable to update. Response: " + response2);
    }

    [Fact]
    public async Task DeleteUserPreferenceHappyPath()
    {
      Msg.Title("User Preference test 6", "Delete user preference successfully");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | " ,
      $"| CreateUserPreferenceRequest | {customerUid} | {prefKeyName}     | {prefKeyUid}     | {userUid}     |  1.0          | some json here |" };
      await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);

      var prefEventArray2 = new[] {
       "| EventType                   | CustomerUID   |",
      $"| DeleteUserPreferenceRequest | {customerUid} |"};
      var queryParams = $"?preferencekeyname={prefKeyName}&preferencekeyuid={prefKeyUid}&userGuid={userUid}";
      var response = await ts.PublishEventToWebApi<ContractExecutionResult>(prefEventArray2, queryParams);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
    }

    [Fact]
    public async Task DeleteUserPreferenceNoExisting()
    {
      Msg.Title("User Preference test 7", "Delete non-existant user preference");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var userUid = RestClient.DEFAULT_USER;
      var prefKeyName = $"My preference key {DateTime.Now.Ticks}";
      var prefKeyUid = Guid.NewGuid();
      var prefEventArray = new[] {
       "| EventType                   | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | TargetUserUID | SchemaVersion | PreferenceJson |",
      $"| CreatePreferenceKeyEvent    | {customerUid} | {prefKeyName}     | {prefKeyUid}     |               |               |                | "};
      await ts.PublishEventCollection<ContractExecutionResult>(prefEventArray);

      var prefEventArray2 = new[] {
       "| EventType                   | CustomerUID   |",
      $"| DeleteUserPreferenceRequest | {customerUid} |"};
      var queryParams = $"?preferencekeyname={prefKeyName}&preferencekeyuid={prefKeyUid}&userGuid={userUid}";
      var response2 = await ts.PublishEventToWebApi<ContractExecutionResult>(prefEventArray2, queryParams, HttpStatusCode.InternalServerError);
      Assert.True(response2 == $"Unable to delete user preference. ", "Response is unexpected. Should fail with unable to delete. Response: " + response2);
    }

  }
}

using System;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using TestUtility;
using Xunit;

namespace WebApiTests
{
  public class PreferenceKeyWebTests : WebApiTestsBase
  {
    [Fact]
    public async Task CreatePreferenceKeyHappyPath()
    {
      Msg.Title("Preference Key test 1", "Create preference key successfully");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyEventArray = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | My preference key | {prefKeyUid}     |" };
      var response = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
    }

    [Fact]
    public async Task CreatePreferenceKeyDuplicateKeyName()
    {
      Msg.Title("Preference Key test 2", "Create duplicate preference key name");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid1 = Guid.NewGuid();
      var prefKeyName = "My preference key";
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName}     | {prefKeyUid1}    |" };
      var response1 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var prefKeyUid2 = Guid.NewGuid(); 
      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName}     | {prefKeyUid2}    |" };
      var response2 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray2);
      Assert.True(response2 == $"Duplicate preference key name. {prefKeyName}", "Response is unexpected. Should fail with duplicate name. Response: " + response2);    
    }

    [Fact]
    public async Task CreatePreferenceKeyDuplicateKeyUid()
    {
      Msg.Title("Preference Key test 3", "Create duplicate preference key UID");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyName1 = "My preference key";
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName1}    | {prefKeyUid}     |" };
      var response1 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var prefKeyName2 = "A new key";
      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName2}     | {prefKeyUid}    |" };
      var response2 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray2);
      Assert.True(response2 == $"Duplicate preference key UID. {prefKeyUid}", "Response is unexpected. Should fail with duplicate UID. Response: " + response2);
    }

    [Fact]
    public async Task UpdatePreferenceKeyHappyPath()
    {
      Msg.Title("Preference Key test 4", "Create preference key then update it successfully");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | My preference key | {prefKeyUid}     |" };
      await ts.PublishEventCollection<PreferenceKeyV1Result>(prefKeyEventArray1);

      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| UpdatePreferenceKeyEvent | {customerUid} | My updated key    | {prefKeyUid}     |" };
      var response = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
    }

    [Fact]
    public async Task UpdatePreferenceKeyDuplicateKeyName()
    {
      Msg.Title("Preference Key test 5", "Update duplicate preference key name");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid1 = Guid.NewGuid();
      var prefKeyName1 = "My preference key";
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName1}     | {prefKeyUid1}    |" };
      var response1 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var prefKeyUid2 = Guid.NewGuid();
      var prefKeyName2 = "Another preference key";
      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName2}     | {prefKeyUid2}    |" };
      var response2 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray2);
      Assert.True(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var prefKeyEventArray3 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| UpdatePreferenceKeyEvent | {customerUid} | {prefKeyName1}     | {prefKeyUid2}    |" };
      var response3 = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray3);
      Assert.True(response3 == $"Duplicate preference key name. {prefKeyName1}", "Response is unexpected. Should fail with duplicate name. Response: " + response3);
    }

    [Fact]
    public async Task UpdatePreferenceKeyNoExisting()
    {
      Msg.Title("Preference Key test 6", "Update non-existant preference key");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyName = "My preference key";
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| UpdatePreferenceKeyEvent | {customerUid} | {prefKeyName}     | {prefKeyUid}     |" };
      var response = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray1);
      Assert.True(response == $"Unable to update preference key. {prefKeyName}", "Response is unexpected. Should fail with unable to update. Response: " + response);
    }

    [Fact]
    public async Task DeletePreferenceKeyWithUserPreference()
    {
      Msg.Title("Preference Key test 7", "Create preference key and user preference, then delete key");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | My preference key | {prefKeyUid}     |" };
      await ts.PublishEventCollection<PreferenceKeyV1Result>(prefKeyEventArray1);

      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| UpdatePreferenceKeyEvent | {customerUid} | My updated key    | {prefKeyUid}     |" };
      var response = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray2);
      Assert.True(response == $"Cannot delete preference key as user preferences exist. {prefKeyUid}", "Response is unexpected. Should fail with user preferences exist. Response: " + response);
    }

    [Fact]
    public async Task DeletePreferenceKeyNoExisting()
    {
      Msg.Title("Preference Key test 8", "Delete non-existant preference key");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyName = "My preference key";
      var prefKeyEventArray = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| DeletePreferenceKeyEvent | {customerUid} |                   | {prefKeyUid}     |" };
      var response = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray);
      Assert.True(response == $"Unable to delete preference key. {prefKeyName}", "Response is unexpected. Should fail with unable to delete. Response: " + response);
    }

    [Fact]
    public async Task DeletePreferenceKeyHappyPath()
    {
      Msg.Title("Preference Key test 9", "Create preference key then delete it successfully");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prefKeyUid = Guid.NewGuid();
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | My preference key | {prefKeyUid}     |" };
      await ts.PublishEventCollection<PreferenceKeyV1Result>(prefKeyEventArray1);

      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| DeletePreferenceKeyEvent | {customerUid} |                   | {prefKeyUid}     |" };
      var response = await ts.PublishEventToWebApi<PreferenceKeyV1Result>(prefKeyEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
    }
  }
}

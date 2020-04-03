using System;
using System.Net;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using TestUtility;
using Xunit;

namespace WebApiTests
{
  public class PreferenceKeyWebTests : WebApiTestsBase
  {
    [Fact]
    public async Task CreatePreferenceKeyThenGetIt()
    {
      Msg.Title("Preference Key test 1", "Create preference key then get it");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prekKeyUid = Guid.NewGuid();
      var prefKeyEventArray = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | My preference key | {prekKeyUid}     |" };
      await ts.PublishEventCollection(prefKeyEventArray);
      await ts.GetPreferenceViaWebApiAndCompareActualWithExpected<PreferenceKeyV1Result>(HttpStatusCode.OK, customerUid, prefKeyEventArray, false);
    }

    [Fact]
    public async Task CreatePreferenceKeyDuplicateKeyName()
    {
      Msg.Title("Preference Key test 2", "Create duplicate preference key name");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prekKeyUid1 = Guid.NewGuid();
      var prefKeyName = "My preference key";
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName}     | {prekKeyUid1}    |" };
      var response1 = await ts.PublishEventToWebApi(prefKeyEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var prekKeyUid2 = Guid.NewGuid(); 
      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName}     | {prekKeyUid2}    |" };
      var response2 = await ts.PublishEventToWebApi(prefKeyEventArray2);
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
      var response1 = await ts.PublishEventToWebApi(prefKeyEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var prefKeyName2 = "A new key";
      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | {prefKeyName2}     | {prefKeyUid}    |" };
      var response2 = await ts.PublishEventToWebApi(prefKeyEventArray2);
      Assert.True(response2 == $"Duplicate preference key UID. {prefKeyUid}", "Response is unexpected. Should fail with duplicate UID. Response: " + response2);
    }

    [Fact]
    public async Task UpdatePreferenceKeyThenGetIt()
    {
      Msg.Title("Preference Key test 4", "Create preference key, update it and get it");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var prekKeyUid = Guid.NewGuid();
      var prefKeyEventArray1 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| CreatePreferenceKeyEvent | {customerUid} | My preference key | {prekKeyUid}     |" };
      await ts.PublishEventCollection(prefKeyEventArray1);

      var prefKeyEventArray2 = new[] {
       "| EventType                | CustomerUID   | PreferenceKeyName | PreferenceKeyUID | ",
      $"| UpdatePreferenceKeyEvent | {customerUid} | My updated key    | {prekKeyUid}     |" };
      await ts.PublishEventCollection(prefKeyEventArray2);
      await ts.GetPreferenceViaWebApiAndCompareActualWithExpected<PreferenceKeyV1Result>(HttpStatusCode.OK, customerUid, prefKeyEventArray2, false);
    }
  }
}

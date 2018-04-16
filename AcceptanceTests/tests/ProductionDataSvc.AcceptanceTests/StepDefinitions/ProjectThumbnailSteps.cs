using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System;
using System.Net;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ProjectThumbnails")]
  public class ProjectThumbnailSteps
  {
    private string uri;
    private string responseRepositoyFileName;
    private Getter<TileResult> tileRequester;

    
    [Given(@"The project thumbnail URI is ""(.*)""")]
    public void GivenTheProjectThumbnailURIIs(string uri)
    {
      this.uri = RaptorClientConfig.CompactionSvcBaseUri + uri;
    }

    [Given(@"the expected response is in the ""(.*)"" respository")]
    public void GivenTheExpectedResponseIsInTheRespository(string fileName)
    {
      responseRepositoyFileName = fileName;
    }

    
    [When(@"I request a Report Tile for project UID ""(.*)""")]
    public void WhenIRequestAReportTileForProjectUID(string projectUid)
    {
      uri += $"?projectUid={projectUid}";
      tileRequester = new Getter<TileResult>(uri, responseRepositoyFileName);
      tileRequester.DoValidRequest(HttpStatusCode.OK);
    }

    [Then(@"The resulting thumbnail should match ""(.*)"" from the response repository within ""(.*)"" percent")]
    public void ThenTheResultingThumbnailShouldMatchFromTheResponseRepositoryWithinPercent(string responseName, int tolerance)
    {
      var imageDifference = Convert.ToDouble(tolerance) / 100;
      var expectedTileData = tileRequester.ResponseRepo[responseName].TileData;
      var actualTileData = tileRequester.CurrentResponse.TileData;
      var expFileName = "Expected_" + ScenarioContext.Current.ScenarioInfo.Title + responseName + ".jpg";
      var actFileName = "Actual_" + ScenarioContext.Current.ScenarioInfo.Title + responseName + ".jpg";
      var diff = Common.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);
      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);
      Console.WriteLine(tileRequester.CurrentResponse);
      Assert.IsTrue(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }





  }
}

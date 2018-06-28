
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionTile")]
  public class CompactionTileSteps
  {
    private Getter<TileResult> tileRequester;
    private string url;

    [Given(@"the Compaction service URI ""(.*)""")]
    public void GivenTheCompactionServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      tileRequester = new Getter<TileResult>(url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      tileRequester.QueryString.Add("ProjectUid", projectUid);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      if (!string.IsNullOrEmpty(filterUid))
        { tileRequester.QueryString.Add("filterUid", filterUid);}
    }

    [Given(@"cutfillDesignUid ""(.*)""")]
    public void GivenCutfillDesignUid(string cutfillDesignUid)
    {
      if (!string.IsNullOrEmpty(cutfillDesignUid))
        { tileRequester.QueryString.Add("cutfillDesignUid", cutfillDesignUid);}
    }


    [Then(@"the result tile should match the ""(.*)"" from the repository within ""(.*)"" percent")]
    public void ThenTheResultTileShouldMatchTheFromTheRepositoryWithin(string resultName, string difference)
    {
      double imageDifference = 0;
      if (!string.IsNullOrEmpty(difference))
      {
        imageDifference = Convert.ToDouble(difference) / 100;
      }    
      var expectedTileData = tileRequester.ResponseRepo[resultName].TileData;
      var actualTileData = tileRequester.CurrentResponse.TileData;
      var expFileName = "Expected_" + ScenarioContext.Current.ScenarioInfo.Title + resultName + ".jpg";
      var actFileName = "Actual_" + ScenarioContext.Current.ScenarioInfo.Title + resultName + ".jpg";
      var diff = Common.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);
      Console.WriteLine("Actual Difference % = " + diff*100); 
      Console.WriteLine("Actual filename = " + actFileName);
      Console.WriteLine(tileRequester.CurrentResponse);
      Assert.IsTrue(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff*100 + "% Expected tiles (" + expFileName +  ") doesn't match actual tiles (" + actFileName + ")");
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      tileRequester.DoValidRequest(url);
    }

    [Given(@"displayMode ""(.*)"" and bbox ""(.*)"" and width ""(.*)"" and height ""(.*)""")]
    public void GivenDisplayModeAndBboxLLAndWidthAndHeight(int mode, string bbox, int width, int height)
    {
      tileRequester.QueryString.Add("mode", mode.ToString());
      tileRequester.QueryString.Add("bbox", bbox);
      tileRequester.QueryString.Add("width", width.ToString());
      tileRequester.QueryString.Add("height", height.ToString());
    }

    [Given(@"a summary volume file with volumeCalcType ""(.*)"" and a topUid ""(.*)"" and a baseUid ""(.*)""")]
    public void GivenAVolumeCalcTypeAndABaseUid(string volumeCalcType, string volumeTopUid, string volumeBaseUid)
    {
      if (!string.IsNullOrEmpty(volumeCalcType))
        { tileRequester.QueryString.Add("volumeCalcType", volumeCalcType);}
      if (!string.IsNullOrEmpty(volumeTopUid))
        { tileRequester.QueryString.Add("volumeTopUid", volumeTopUid);}
      if (!string.IsNullOrEmpty(volumeBaseUid))
        { tileRequester.QueryString.Add("volumeBaseUid", volumeBaseUid);}
    }
  }
}

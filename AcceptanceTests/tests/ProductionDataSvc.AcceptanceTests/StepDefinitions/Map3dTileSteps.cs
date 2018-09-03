using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "Map3dTile")]
  public class Map3dTileSteps
  {
    private string _url;
    private Getter<TileResult> _tileRequester;

    [Given(@"the Map3d service URI ""(.*)""")]
    public void GivenTheCompactionServiceURI(string url)
    {
      this._url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      _tileRequester = new Getter<TileResult>(_url, resultFileName);
    }

    [Given(@"projectUid ""(.*)""")]
    public void GivenProjectUid(string projectUid)
    {
      _tileRequester.QueryString.Add("ProjectUid", projectUid);
    }

    [Given(@"designUid ""(.*)""")]
    public void GivenDesignUid(string designUid)
    {
      _tileRequester.QueryString.Add("DesignUid", designUid);
    }

    [Given(@"filterUid ""(.*)""")]
    public void GivenFilterUid(string filterUid)
    {
      if (!string.IsNullOrEmpty(filterUid))
      {
        _tileRequester.QueryString.Add("filterUid", filterUid);
      }
    }

    [Given(@"mode ""(.*)"" and type ""(.*)"" and bbox ""(.*)"" and width ""(.*)"" and height ""(.*)""")]
    public void GivenQueryAttributes(int mode, int type, string bbox, int width, int height)
    {
      _tileRequester.QueryString.Add("mode", mode.ToString());
      _tileRequester.QueryString.Add("type", type.ToString());
      _tileRequester.QueryString.Add("bbox", bbox);
      _tileRequester.QueryString.Add("width", width.ToString());
      _tileRequester.QueryString.Add("height", height.ToString());
    }

    [Then(@"the result should be server errror - not implemented")]

    [Then(@"the result tile should match the ""(.*)"" from the repository within ""(.*)"" percent")]
    public void ThenTheResultTileShouldMatchTheFromTheRepositoryWithin(string resultName, string difference = "0")
    {
      var imageDifference = Convert.ToDouble(difference) / 100;

      var expectedTileData = _tileRequester.ResponseRepo[resultName].TileData;
      var actualTileData = _tileRequester.CurrentResponse.TileData;

      var expFileName = "Expected_" + ScenarioContext.Current.ScenarioInfo.Title + resultName + ".jpg";
      var actFileName = "Actual_" + ScenarioContext.Current.ScenarioInfo.Title + resultName + ".jpg";

      var diff = Common.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);

      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);
      Console.WriteLine(_tileRequester.CurrentResponse);

      Assert.IsTrue(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }

    [Then(@"I request something that is not completed the response HTTP code should be ""(.*)""")]
    public void WhileIRequestNotImplemented(int statusCode)
    {
      _tileRequester.DoInvalidRequest(_url, (HttpStatusCode)statusCode);
    }

    [When(@"I request result")]
    public void WhenIRequestResult()
    {
      _tileRequester.DoValidRequest(_url);
    }
  }
}

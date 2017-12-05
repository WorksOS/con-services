using System;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ReportTile")]
  public class ReportTileSteps
  {
    private string url;
    private string projectUid;
    private string filterUid;
    private string overlayType;
    private string mapType;
    private int? mode;
    private int width = 256;
    private int height = 256;
    private string volumeCalcType;
    private string volumeBaseUid;
    private string volumeTopUid;
    private string cutFillDesignUid;
    private Getter<TileResult> tileRequester;

    [Given(@"the Report Tile service URI ""(.*)""")]
    public void GivenTheReportTileServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a filterUid ""(.*)""")]
    public void GivenAFilterUid(string filterUid)
    {
      this.filterUid = filterUid;
    }

    [Given(@"an overlayType ""(.*)""")]
    public void GivenAnOverlayType(string overlayType)
    {
      this.overlayType = overlayType;
    }

    [Given(@"a mapType ""(.*)""")]
    public void GivenAMapType(string mapType)
    {
      this.mapType = mapType;
    }

    [Given(@"a mode ""(.*)""")]
    public void GivenAMode(int mode)
    {
      this.mode = mode;
    }

    [Given(@"a width ""(.*)"" and a height ""(.*)""")]
    public void GivenAWidthAndAHeight(int width, int height)
    {
      this.width = width;
      this.height = height;
    }

    [Given(@"a volumeCalcType ""(.*)""")]
    public void GivenAVolumeCalcType(string volumeCalcType)
    {
      this.volumeCalcType = volumeCalcType;
    }

    [Given(@"a volumeTopUid ""(.*)""")]
    public void GivenAVolumeTopUid(string volumeTopUid)
    {
      this.volumeTopUid = volumeTopUid;
    }

    [Given(@"a cutFillDesignUid ""(.*)""")]
    public void GivenACutFillDesignUid(string cutFillDesignUid)
    {
      this.cutFillDesignUid = cutFillDesignUid;
    }

    [Given(@"a volumeBaseUid ""(.*)""")]
    public void GivenAVolumeBaseUid(string volumeBaseUid)
    {
      this.volumeBaseUid = volumeBaseUid;
    }

    [When(@"I request a Report Tile")]
    public void WhenIRequestAReportTile()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoValidRequest();
    }

    [Then(@"the Report Tile result should be")]
    public void ThenTheReportTileResultShouldBe(string multilineText)
    {
      TileResult expected = JsonConvert.DeserializeObject<TileResult>(multilineText);
      Assert.AreEqual(expected, tileRequester.CurrentResponse);
    }

    [Then(@"the Report Tile result image should be match within ""(.*)"" percent")]
    public void ThenTheReportTileResultImageShouldBeMatchWithinPercent(string difference, string multilineText)
    {
      double imageDifference = 0;
      if (!string.IsNullOrEmpty(difference))
      {
        imageDifference = Convert.ToDouble(difference) / 100;
      }
      TileResult expected = JsonConvert.DeserializeObject<TileResult>(multilineText);
      var expectedTileData = expected.TileData;
      var actualTileData = tileRequester.CurrentResponse.TileData;
      var expFileName = "Expected_" + ScenarioContext.Current.ScenarioInfo.Title + ".jpg";
      var actFileName = "Actual_" + ScenarioContext.Current.ScenarioInfo.Title + ".jpg";
      var diff = Common.CompareImagesAndGetDifferencePercent(expectedTileData, actualTileData, expFileName, actFileName);
      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);
     // Console.WriteLine(tileRequester.CurrentResponse);
      Assert.IsTrue(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");
    }


    [When(@"I request a Report Tile Expecting BadRequest")]
    public void WhenIRequestAReportTileExpectingBadRequest()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.AreEqual(errorCode, tileRequester.CurrentResponse.Code);
      Assert.AreEqual(message, tileRequester.CurrentResponse.Message);
    }

    private string MakeUrl()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append($"{url}?projectUid={projectUid}&width={width}&height={height}&overlays={overlayType}");
      if (!string.IsNullOrEmpty(filterUid))
      {
        sb.Append($"&filterUid={filterUid}");
      }
      if (!string.IsNullOrEmpty(mapType))
      {
        sb.Append($"&mapType={mapType}");
      }
      if (mode.HasValue)
      {
        sb.Append($"&mode={mode}");
      }
      if (!string.IsNullOrEmpty(volumeCalcType))
      {
        sb.Append($"&volumeCalcType={volumeCalcType}");
      }
      if (!string.IsNullOrEmpty(volumeBaseUid))
      {
        sb.Append($"&volumeBaseUid={volumeBaseUid}");
      }
      if (!string.IsNullOrEmpty(volumeTopUid))
      {
        sb.Append($"&volumeTopUid={volumeTopUid}");
      }
      if (!string.IsNullOrEmpty(cutFillDesignUid))
      {
        sb.Append($"&cutFillDesignUid={cutFillDesignUid}");
      }
      return sb.ToString();
    }
  }
}

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
    private string language;
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
    public void GivenAMode(string mode)
    {
      if (!string.IsNullOrEmpty(mode))
      this.mode = Convert.ToInt32(mode);
    }

    [Given(@"a language ""(.*)""")]
    public void GivenALanguage(string language)
    {
      this.language = language;
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

    [Given(@"I set width and height to ""(.*)""")]
    public void GivenISetWidthAndHeightTo(int widthheight)
    {
      ScenarioContext.Current.Pending();
    }



    [When(@"I request a Report Tile and the result file ""(.*)""")]
    public void WhenIRequestAReportTileAndTheResultFile(string resultFile)
    {
      tileRequester = new Getter<TileResult>(MakeUrl(), resultFile);
      tileRequester.DoValidRequest();
    }

    [When(@"I request a Report Tile")]
    public void WhenIRequestAReportTile()
    {
      tileRequester = new Getter<TileResult>(MakeUrl());
      tileRequester.DoValidRequest();
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
      Console.WriteLine(tileRequester.CurrentResponse);
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

    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string resultFileName)
    {
      tileRequester = new Getter<TileResult>(url, resultFileName);
    }

    [Then(@"the result tile should match the ""(.*)"" from the repository within ""(.*)"" percent")]
    public void ThenTheResultTileShouldMatchTheFromTheRepositoryWithinPercent(string resultName, string difference)
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
      Console.WriteLine("Actual Difference % = " + diff * 100);
      Console.WriteLine("Actual filename = " + actFileName);
      Console.WriteLine(tileRequester.CurrentResponse);
      Assert.IsTrue(Math.Abs(diff) < imageDifference, "Actual Difference:" + diff * 100 + "% Expected tiles (" + expFileName + ") doesn't match actual tiles (" + actFileName + ")");

    }


    private string MakeUrl()
    {
      var sb = new StringBuilder();
      sb.Append($"{url}?projectUid={projectUid}&width={width}&height={height}");
      if (!string.IsNullOrEmpty(overlayType))
      {
        if (overlayType.Contains(","))
        {
          var otArray = overlayType.Split(',');
          foreach (var ot in otArray)
          {
            sb.Append($"&overlays={ot.Trim()}");
          }
        }
        else
        {
          sb.Append($"&overlays={overlayType}");
        }
      }
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
      if (!string.IsNullOrEmpty(language))
      {
        sb.Append($"&language{language}");
      }
      return sb.ToString();
    }
  }
}

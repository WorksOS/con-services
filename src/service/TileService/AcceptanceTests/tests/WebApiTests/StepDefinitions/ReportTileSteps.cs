using System;
using System.Net;
using System.Text;
using WebApiTests.Models;
using WebApiTests.Utilities;
using Xunit;
using Xunit.Gherkin.Quick;

namespace WebApiTests.StepDefinitions
{
  [FeatureFile("ReportTile.feature")]//path relative to output folder
  public sealed class ReportTileSteps : StepsBase
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
    private Getter<byte[]> tileRequesterGood;
    private Getter<RequestResult> tileRequesterBad;
    private byte[] currentResponse;


    [Given(@"the Report Tile service URI ""(.*)""")]
    public void GivenTheReportTileServiceURI(string url)
    {
      this.url = TileClientConfig.TileSvcBaseUri + url;
    }

    [And(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [And(@"a filterUid ""(.*)""")]
    public void GivenAFilterUid(string filterUid)
    {
      this.filterUid = filterUid;
    }

    [And(@"an overlayType ""(.*)""")]
    public void GivenAnOverlayType(string overlayType)
    {
      this.overlayType = overlayType;
    }

    [And(@"a mapType ""(.*)""")]
    public void GivenAMapType(string mapType)
    {
      this.mapType = mapType;
    }

    [And(@"a mode ""(.*)""")]
    public void GivenAMode(string mode)
    {
      if (!string.IsNullOrEmpty(mode))
      this.mode = Convert.ToInt32(mode);
    }

    [And(@"a language ""(.*)""")]
    public void GivenALanguage(string language)
    {
      this.language = language;
    }

    [And(@"a width ""(.*)"" and a height ""(.*)""")]
    public void GivenAWidthAndAHeight(int width, int height)
    {
      this.width = width;
      this.height = height;
    }

    [And(@"a volumeCalcType ""(.*)""")]
    public void GivenAVolumeCalcType(string volumeCalcType)
    {
      this.volumeCalcType = volumeCalcType;
    }

    [And(@"a volumeTopUid ""(.*)""")]
    public void GivenAVolumeTopUid(string volumeTopUid)
    {
      this.volumeTopUid = volumeTopUid;
    }

    [And(@"a cutFillDesignUid ""(.*)""")]
    public void GivenACutFillDesignUid(string cutFillDesignUid)
    {
      this.cutFillDesignUid = cutFillDesignUid;
    }

    [And(@"a volumeBaseUid ""(.*)""")]
    public void GivenAVolumeBaseUid(string volumeBaseUid)
    {
      this.volumeBaseUid = volumeBaseUid;
    }

    [When(@"I request a Report Tile and the result file ""(.*)""")]
    public void WhenIRequestAReportTileAndTheResultFile(string resultFile)
    {
      var uri = MakeUrl();
      tileRequesterGood = new Getter<byte[]>(uri, resultFile);
      currentResponse = tileRequesterGood.DoRequestWithStreamResponse(uri);
    }

    [When(@"I request a Report Tile Expecting BadRequest")]
    public void WhenIRequestAReportTileExpectingBadRequest()
    {
      tileRequesterBad = new Getter<RequestResult>(MakeUrl());
      tileRequesterBad.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.Equal(errorCode, tileRequesterBad.CurrentResponse?.Code);
      Assert.Equal(message, tileRequesterBad.CurrentResponse?.Message);
    }

    [Then(@"the result tile should match the ""(.*)"" from the repository within ""(.*)"" percent")]
    public void ThenTheResultTileShouldMatchTheFromTheRepositoryWithinPercent(string resultName, string difference)
    {
      CompareExpectedAndActualTiles(resultName, difference, tileRequesterGood.ResponseRepo[resultName], currentResponse);
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
        sb.Append($"&language={language}");
      }
      return sb.ToString();
    }
  }
}

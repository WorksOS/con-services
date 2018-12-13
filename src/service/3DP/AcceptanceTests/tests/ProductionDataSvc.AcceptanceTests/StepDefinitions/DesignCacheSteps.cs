using System.IO;
using System.Net;
using System.Net.Http;
using ProductionDataSvc.AcceptanceTests.Models;
using ProductionDataSvc.AcceptanceTests.Utils;
using Xunit;
using Xunit.Gherkin.Quick;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [FeatureFile("DesignCache.feature")]
  public class DesignCacheSteps : Feature
  {
    private Poster<DesignNameRequest, DummyRequestResult> designCacheDeleter;

    [Given(@"the DeleteDesignCacheFile service URI ""(.*)"", a project (.*) and a file named ""(.*)""")]
    public void GivenTheDeleteDesignCacheFileServiceURIAProjectAndAFileNamed(string uri, long projectID, string designName)
    {
      designCacheDeleter = new Poster<DesignNameRequest, DummyRequestResult>(
          RestClient.Productivity3DServiceBaseUrl + uri,
          new DesignNameRequest { ProjectId = projectID, DesignFilename = designName });
    }

    [Given(@"the following Summary Volumes request is sent to ""(.*)"" to make sure the design file is downloaded if required")]
    public void GivenTheFollowingSummaryVolumesRequestIsSentToToMakeSureTheDesignFileIsDownloadedIfRequired(string sVuri, string svRequestStr)
    {
      string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
      string fullDesignFileCachePath = Path.Combine(designFileCachePath,
          designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

      if (!File.Exists(fullDesignFileCachePath))
      {
        var result = RestClient.SendHttpClientRequest(
          RestClient.Productivity3DServiceBaseUrl,
          sVuri,
          HttpMethod.Post,
          MediaTypes.JSON,
          MediaTypes.JSON,
          svRequestStr).Result;

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
      }
    }

    [When(@"I delete this file")]
    public void WhenIDeleteThisFile()
    {
      designCacheDeleter.DoRequest();
    }

    [Then(@"the file should no longer exist in the design cache")]
    public void ThenTheFileShouldNoLongerExistInTheDesignCache()
    {
      string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
      string fullDesignFileCachePath = Path.Combine(designFileCachePath,
          designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

      Assert.False(File.Exists(fullDesignFileCachePath), $"Expected file {fullDesignFileCachePath} deleted, but it's still there.");
    }

    [Given(@"the file does not already exist in the design cache")]
    public void GivenTheFileDoesNotAlreadyExistInTheDesignCache()
    {
      string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
      string fullDesignFileCachePath = Path.Combine(designFileCachePath,
          designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

      if (File.Exists(fullDesignFileCachePath))
      {
        try
        {
          designCacheDeleter.DoRequest();
        }
        catch
        {
          //Logger.Error(e, Logger.ContentType.Error);
        }
      }
    }

    [When(@"the following Summary Volumes request is sent to ""(.*)""")]
    public void WhenTheFollowingSummaryVolumesRequestIsSentTo(string sVuri, string svRequestStr)
    {
      var result = RestClient.SendHttpClientRequest(
         RestClient.Productivity3DServiceBaseUrl,
         sVuri,
         HttpMethod.Post,
         MediaTypes.JSON,
         MediaTypes.JSON,
         svRequestStr).Result;

      Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Then(@"the file should be automatically downloaded into the design cache")]
    public void ThenTheFileShouldBeAutomaticallyDownloadedIntoTheDesignCache()
    {
      string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
      string fullDesignFileCachePath = Path.Combine(designFileCachePath,
          designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

      Assert.True(File.Exists(fullDesignFileCachePath), $"Expected file {fullDesignFileCachePath} downloaded, but it's not there.");
    }
  }
}

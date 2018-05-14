using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionProfile")]
  public class CompactionProfileSteps
  {
    private Getter<CompactionProfileResult<CompactionProfileDataResult>> profileRequester;
    private string resultFileName = string.Empty;
    private string url;
    private string projectUid;
    private string queryParameters = string.Empty;

    [Given(@"the Compaction Profile service URI ""(.*)""")]
    public void GivenTheCompactionProfileServiceUri(string uri)
    {
      url = RaptorClientConfig.CompactionSvcBaseUri + uri;
    }
    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectId)
    {
      projectUid = projectId;
    }

    [Given(@"a startLatDegrees ""(.*)"" and a startLonDegrees ""(.*)"" and an endLatDegrees ""(.*)"" And an endLonDegrees ""(.*)""")]
    public void GivenAStartLatDegreesAndAStartLonDegreesAndAnEndLatDegreesAndAnEndLonDegrees(Decimal startLatDegrees, Decimal startLonDegrees, Decimal endLatDegrees, Decimal endLonDegrees)
    {
      queryParameters = string.Format("&startLatDegrees={0}&startLonDegrees={1}&endLatDegrees={2}&endLonDegrees={3}",
        startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees);
    }

    [Given(@"a cutfillDesignUid ""(.*)""")]
    public void GivenACutfillDesignUid(string cutfillDesignUid)
    {
      queryParameters += string.Format("&cutfillDesignUid={0}", cutfillDesignUid);
    }


    [Given(@"a volumeCalcType ""(.*)"" and a topUid ""(.*)"" and a baseUid ""(.*)""")]
    public void GivenAVolumeCalcTypeAndATopUidAndABaseUid(string volumeCalcType, string volumeTopUid, string volumeBaseUid)
    {
      queryParameters += string.Format("&volumeCalcType={0}&volumeTopUid={1}&volumeBaseUid={2}", volumeCalcType, volumeTopUid, volumeBaseUid);
    }
    
    [When(@"I request a Compaction Profile")]
    public void WhenIRequestACompactionProfile()
    {
      profileRequester = Getter<CompactionProfileResult<CompactionProfileDataResult>>.GetIt<CompactionProfileResult<CompactionProfileDataResult>>(url, projectUid, queryParameters);
    }


    [Given(@"the result file ""(.*)""")]
    public void GivenTheResultFile(string expectedResultFile)
    {
      resultFileName = expectedResultFile;
    }

    [Then(@"the Compaction Profile result should be match expected ""(.*)""")]
    public void ThenTheCompactionProfileResultShouldBeMatchExpected(string resultSelector)
    {
      var expectedData = new Getter<CompactionProfileResultTest>(url, resultFileName).ResponseRepo[resultSelector];
      var actualData = profileRequester.CurrentResponse;
      Assert.AreEqual(expectedData, actualData, "Expected does not match actual for profile line");      
    }
  }
}

using System;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionProfile")]
  public class CompactionProfileSteps
  {
    private Getter<CompactionProfileResult<CompactionProfileDataResult>> profileRequester;

    private string url;
    private string projectUid;
    private string queryParameters = string.Empty;

    [Given(@"the Compaction Profile service URI ""(.*)""")]
    public void GivenTheCompactionProfileServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }
    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
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
      profileRequester = Getter<CompactionProfileResult<CompactionProfileDataResult>>.GetIt<CompactionProfileResult<CompactionProfileDataResult>>(this.url, this.projectUid, this.queryParameters);
    }

    [Then(@"the Compaction Profile should be")]
    public void ThenTheCompactionProfileShouldBe(string multilineText)
    {
      profileRequester.CompareIt<CompactionProfileResult<CompactionProfileDataResult>>(multilineText);
    }





  }
}

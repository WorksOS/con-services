
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
      tileRequester.QueryString.Add("filterUid", filterUid);
    }

    [Then(@"the result should match the ""(.*)"" from the repository")]
    public void ThenTheResultShouldMatchTheFromTheRepository(string resultName)
    {
      Assert.AreEqual(tileRequester.ResponseRepo[resultName], tileRequester.CurrentResponse);
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
  }
}

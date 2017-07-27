
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
    private string projectUid;
    private string queryParameters = string.Empty;

    [Given(@"the Compaction Tiles service URI ""(.*)""")]
    public void GivenTheCompactionTilesServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a displayMode ""(.*)"" and a bbox ""(.*)"" and a width ""(.*)"" and a height ""(.*)""")]
    public void GivenADisplayModeAndABboxAndAWidthAndAHeight(int mode, string bbox, int width, int height)
    {
      queryParameters = string.Format("&mode={0}&BBOX={1}&WIDTH={2}&HEIGHT={3}",
        mode, bbox, width, height);
    }

    [When(@"I request a Tile")]
    public void WhenIRequestATile()
    {
      tileRequester = Getter<TileResult>.GetIt<TileResult>(this.url, this.projectUid, this.queryParameters);
    }

    [Then(@"the Tile result should be")]
    public void ThenTheTileResultShouldBe(string multilineText)
    {
      tileRequester.CompareIt<TileResult>(multilineText);
    }
  }
}

using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  public abstract class BaseCompactionSteps
  {
    protected string url;
    protected string operation;

    [Given(@"the Compaction service URI ""(.*)"" for operation ""(.*)""")]
    public void GivenTheCompactionServiceURIForOperation(string url, string operation)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
      this.operation = operation;
    }


    protected string TEST_FAIL_MESSAGE = "Unsupported test's operation";
  }
}

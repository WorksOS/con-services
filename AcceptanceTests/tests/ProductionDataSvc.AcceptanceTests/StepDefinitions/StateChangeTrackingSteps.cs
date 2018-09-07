using TechTalk.SpecFlow;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "StateChangeTracking")]
  public class StateChangeTrackingSteps
  {
    private Poster<CompactionTagFilePostParameter, CompactionTagFilePostResult> tagFilePoster;
    private Getter<GetMachinesResult> machineStatusGetter;

    // In v2 overriding target MahineID/legacyAssetID is no longer permitted.
    //  this makes testing an actual stateChange very difficult as we'd need a JohnDoe tagFile
    //       and a landfill/PM project etc etc, which isn't covered in the Golden Data set.
    //  So the most this can do is test that RaptorClient gets TFA authorization, submits the tagFile,
    //      and we are able to query the machine status (although it will be its very latest status, not from this old tagfile)
    //
    //In this scenario, where the tag file has a RadioSerial, TFA will be able to resolve to this LegacyAssetID, and store the file under that location.
    // In a tag file with no RadioSerial i.e. JohnDoe, it will contain a numeric MachineId (huge number) which it will use to store the file under (as TFA has no way to identify any legacyAssetID)
    //
    // Note that the 3dp service runs against a running RaptorClient on Dev.
    //     This means that if you run this test within 30 seconds, you will get this error:
    //        Message: Assert.AreEqual failed. Expected:<OK>. Actual:<BadRequest>. Expected OK,
    //                 but got BadRequest instead.
    //                 Message was {"Code":2018,"Message":"Failed to process tagfile with error: OnProcessTAGFile. TAG file already exists in data model's processing archival queue."}

    private long _legacyAssetId = 1219470261494388; 
    private GetMachinesResult firstDotMachineStatus;

    [Given(@"the Tag service URI ""(.*)"", Tag request repo file ""(.*)""")]
    public void GivenTheTagServiceURITagRequestRepoFile(string uri, string requestFile)
    {
      uri = RaptorClientConfig.TagSvcBaseUri + uri;
      tagFilePoster = new Poster<CompactionTagFilePostParameter, CompactionTagFilePostResult>(uri, requestFile, null);
    }

    [Given(@"the Machine service URI ""(.*)"", Machine result repo file ""(.*)""")]
    public void GivenTheMachineServiceURIMachineResultRepoFile(string uri, string resultFile)
    {
      uri = RaptorClientConfig.ProdSvcBaseUri + uri + _legacyAssetId;
      machineStatusGetter = new Getter<GetMachinesResult>(uri, resultFile);
    }

    [When(@"I post Tag file ""(.*)"" from the Tag request repo")]
    public void WhenIPostTagFileFromTheTagRequestRepo(string paramName)
    {
      tagFilePoster.CurrentRequest = tagFilePoster.RequestRepo[paramName];
      tagFilePoster.DoValidRequest();
      Thread.Sleep(8000);
    }

    [When(@"I get and save the machine detail in one place")]
    public void WhenIGetAndSaveTheMachineDetailInOnePlace()
    {
      firstDotMachineStatus = machineStatusGetter.DoValidRequest();
    }

    [Then(@"the first saved machine detail should match ""(.*)"" result from the Machine result repo")]
    public void ThenTheFirstSavedMachineDetailShouldMatchResultFromTheMachineResultRepo(string resultName)
    {
      if (firstDotMachineStatus.MachineStatuses.Length < 1)
        Assert.Fail(string.Format("Unable to get machine status {0}", firstDotMachineStatus));

      // Need to ignore assetID in validation
      GetMachinesResult expectedFirstDotMachineStatus = machineStatusGetter.ResponseRepo[resultName];
      // expectedFirstDotMachineStatus.MachineStatuses[0].AssetId = firstDotMachineStatus.MachineStatuses[0].AssetId;

      Assert.AreEqual(expectedFirstDotMachineStatus, firstDotMachineStatus);
    }
  }
}

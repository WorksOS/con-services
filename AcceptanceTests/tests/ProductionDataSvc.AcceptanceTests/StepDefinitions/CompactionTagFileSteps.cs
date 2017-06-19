using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "CompactionTagFile")]
  public class CompactionTagFileSteps
  {
    private Poster<CompactionTagFilePostParameter, RequestResult> tagPoster;

    [Given(@"the Compaction Tag file service URI ""(.*)"" and request repo ""(.*)""")]
    public void GivenTheCompactionTagFileServiceURIAndRequestRepo(string uri, string requestFile)
    {
      uri = RaptorClientConfig.TagSvcBaseUri + uri;
      tagPoster = new Poster<CompactionTagFilePostParameter, RequestResult>(uri, requestFile);
    }
        
    [When(@"I POST a compaction tag file with code (.*) from the repository")]
    public void WhenIPOSTACompactionTagFileWithCodeFromTheRepository(int code)
    {
      tagPoster.DoValidRequest(code.ToString(), HttpStatusCode.BadRequest);
    }

    [Then(@"the Tag File Service response should contain Code (.*) and Message ""(.*)""")]
    public void ThenTheTagFileServiceResponseShouldContainCodeAndMessage(int code, string message)
    {
      Assert.IsTrue(tagPoster.CurrentResponse.Code == code && tagPoster.CurrentResponse.Message == message,
        string.Format("Expected Code {0} and Message {1}, but received {2} and {3} instead.",
          code, message, tagPoster.CurrentResponse.Code, tagPoster.CurrentResponse.Message));
    }
  }
}

using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "ProjectSettings")]
  public class ProjectSettingsSteps
  {

    private string url;
    private string projectUid;
    private string projectSettings;
    private byte settingsType;
    private Getter<RequestResult> settingsValidationRequester;


    [Given(@"the Project Settings Validation service URI ""(.*)""")]
    public void GivenTheProjectSettingsValidationServiceURI(string url)
    {
      this.url = RaptorClientConfig.CompactionSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a projectSettings ""(.*)""")]
    public void GivenAProjectSettings(string projectSettings)
    {
      this.projectSettings = projectSettings;
    }

    [Given(@"a settingsType ""(.*)""")]
    public void GivenAProjectSettings(byte settingsType)
    {
      this.settingsType = settingsType;
    }
    
    [Given(@"a projectSettings \(multiline\)")]
    public void GivenAProjectSettingsMultiline(string multilineText)
    {
      this.projectSettings = multilineText;
    }

    [When(@"I request settings validation")]
    public void WhenIRequestSettingsValidation()
    {
      settingsValidationRequester = new Getter<RequestResult>(MakeUrl);
      settingsValidationRequester.DoValidRequest();
    }

    [Then(@"the settings validation result should be")]
    public void ThenTheSettingsValidationResultShouldBe(string multilineText)
    {
      RequestResult expected = JsonConvert.DeserializeObject<RequestResult>(multilineText);
      Assert.IsTrue(expected.Code == settingsValidationRequester.CurrentResponse.Code && expected.Message == settingsValidationRequester.CurrentResponse.Message);
    }

    [When(@"I request settings validation expecting bad request")]
    public void WhenIRequestSettingsValidationExpectingBadRequest()
    {
      settingsValidationRequester = new Getter<RequestResult>(MakeUrl);
      settingsValidationRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.AreEqual(errorCode, settingsValidationRequester.CurrentResponse.Code);
      Assert.AreEqual(message, settingsValidationRequester.CurrentResponse.Message);
    }

    [Then(@"the result should contain code (.*) and message ""(.*)""")]
    public void ThenTheResultShouldContainCodeAndMessage(int code, string msg)
    {
      Assert.IsTrue(
        settingsValidationRequester.CurrentResponse.Code == code &&
        settingsValidationRequester.CurrentResponse.Message == msg,
        $"Expected to see code {code} and message {msg}, but got {settingsValidationRequester.CurrentResponse.Code} and {settingsValidationRequester.CurrentResponse.Message} instead.");
    }

    private string MakeUrl
    {
      get { return $"{url}?projectUid={projectUid}&projectSettings={projectSettings}&settingsType={settingsType}"; }
    }

  }
}

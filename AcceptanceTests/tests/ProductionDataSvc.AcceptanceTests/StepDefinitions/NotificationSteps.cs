using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding]
  public class NotificationSteps
  {
    private string url;
    private string projectUid;
    private string fileDescriptor;
    private long fileId;

    private Getter<RequestResult> fileNotificationRequester;

    [Given(@"the Add File Notification service URI ""(.*)""")]
    public void GivenTheAddFileNotificationServiceURI(string url)
    {
      this.url = RaptorClientConfig.NotificationSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a fileDescriptor ""(.*)""")]
    public void GivenAFileDescriptor(string fileDescriptor)
    {
      this.fileDescriptor = fileDescriptor;
    }
        
    [Given(@"a fileId ""(.*)""")]
    public void GivenAFileId(int fileId)
    {
      this.fileId = fileId;
    }
        
    [When(@"I request Add File Notification")]
    public void WhenIRequestAddFileNotification()
    {
      this.url = string.Format("{0}?projectUid={1}&filedescriptor={2}&fileId={3}", url, projectUid, fileDescriptor, fileId);
      fileNotificationRequester = new Getter<RequestResult>(this.url);
      fileNotificationRequester.DoValidRequest();
    }
        
    [When(@"I request Delete File Notification")]
    public void WhenIRequestDeleteFileNotification()
    {
        ScenarioContext.Current.Pending();
    }
        
    [Then(@"the Add File Notification result should be")]
    public void ThenTheAddFileNotificationResultShouldBe(string multilineText)
    {
      RequestResult expected = JsonConvert.DeserializeObject<RequestResult>(multilineText);
      Assert.AreEqual(expected, fileNotificationRequester.CurrentResponse);
    }

    [Then(@"the Delete File Notification result should be")]
    public void ThenTheDeleteFileNotificationResultShouldBe(string multilineText)
    {
        ScenarioContext.Current.Pending();
    }
  }
}

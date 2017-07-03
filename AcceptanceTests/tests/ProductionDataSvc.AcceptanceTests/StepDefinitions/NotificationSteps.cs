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
    private string fileUid;

    private Getter<RequestResult> fileNotificationRequester;

    [Given(@"the Add File Notification service URI ""(.*)""")]
    public void GivenTheAddFileNotificationServiceURI(string url)
    {
      this.url = RaptorClientConfig.NotificationSvcBaseUri + url;
    }

    [Given(@"the Delete File Notification service URI ""(.*)""")]
    public void GivenTheDeleteFileNotificationServiceURI(string url)
    {
      this.url = RaptorClientConfig.NotificationSvcBaseUri + url;
    }

    [Given(@"a projectUid ""(.*)""")]
    public void GivenAProjectUid(string projectUid)
    {
      this.projectUid = projectUid;
    }

    [Given(@"a filespaceId ""(.*)"" and a path ""(.*)"" and a fileName ""(.*)""")]
    public void GivenAFilespaceIdAndAPathAndAFileName(string filespaceId, string path, string fileName)
    {
      this.fileDescriptor = "{\"filespaceId\":\"" + filespaceId + "\",\"path\":\"" + path + "\",\"fileName\":\"" + fileName + "\"}";
    }

    [Given(@"a fileId ""(.*)""")]
    public void GivenAFileId(int fileId)
    {
      this.fileId = fileId;
    }

    [Given(@"a fileUid ""(.*)""")]
    public void GivenAFileUid(string fileUid)
    {
      this.fileUid = fileUid;
    }


    [When(@"I request File Notification")]
    public void WhenIRequestFileNotification()
    {
      this.url = string.Format("{0}?projectUid={1}&filedescriptor={2}&fileId={3}&fileUid={4}", url, projectUid, fileDescriptor, fileId, fileUid);
      fileNotificationRequester = new Getter<RequestResult>(this.url);
      fileNotificationRequester.DoValidRequest();
    }

    [Then(@"the File Notification result should be")]
    public void ThenTheFileNotificationResultShouldBe(string multilineText)
    {
      RequestResult expected = JsonConvert.DeserializeObject<RequestResult>(multilineText);
      Assert.IsTrue(expected.Code == fileNotificationRequester.CurrentResponse.Code && expected.Message == fileNotificationRequester.CurrentResponse.Message);
    }

  }
}

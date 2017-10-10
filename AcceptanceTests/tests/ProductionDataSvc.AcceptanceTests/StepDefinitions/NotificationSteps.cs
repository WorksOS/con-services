using System;
using System.IO;
using System.Net;
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
    private int fileTypeId = 0;

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
      //1 = DesignSurface, 0 = Linework
      this.fileTypeId = Path.GetExtension(fileName).ToLower() == ".ttm" ? 1 : 0;
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
      MakeUrl();
      fileNotificationRequester = new Getter<RequestResult>(this.url);
      fileNotificationRequester.DoValidRequest();
    }

    [Then(@"the File Notification result should be")]
    public void ThenTheFileNotificationResultShouldBe(string multilineText)
    {
      RequestResult expected = JsonConvert.DeserializeObject<RequestResult>(multilineText);
      Assert.IsTrue(expected.Code == fileNotificationRequester.CurrentResponse.Code && expected.Message == fileNotificationRequester.CurrentResponse.Message);
    }

    [When(@"I request File Notification Expecting BadRequest")]
    public void WhenIRequestADxfTileExpectingBadRequest()
    {
      MakeUrl();
      fileNotificationRequester = new Getter<RequestResult>(this.url);
      fileNotificationRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.AreEqual(errorCode, fileNotificationRequester.CurrentResponse.Code);
      Assert.AreEqual(message, fileNotificationRequester.CurrentResponse.Message);
    }

    private void MakeUrl()
    {
      this.url = string.Format("{0}?projectUid={1}&filedescriptor={2}&fileId={3}&fileUid={4}&fileType={5}", url, projectUid, fileDescriptor, fileId, fileUid, fileTypeId);
    }

  }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using System.IO;
using System.Net;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature = "Notification")]
  public class NotificationSteps
  {
    private string url;
    private string projectUid;
    private string fileDescriptor;
    private long fileId;
    private string fileUid;
    private int fileTypeId = 0;
    private int dxfUnitsType = -1;

    private Getter<AddFileResult> addFileNotificationRequester;
    private Getter<RequestResult> deleteFileNotificationRequester;

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
      //1 = DesignSurface, 0 = Linework, 3 = Alignment
      var ext = Path.GetExtension(fileName).ToLower();
      switch (ext)
      {
        case ".ttm":
          this.fileTypeId = 1;
          break;
        case ".svl":
          this.fileTypeId = 3;
          break;
        default:
          this.fileTypeId = 0;
          break;
      }
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

    [Given(@"a dxfUnitsType ""(.*)""")]
    public void GivenADxfUnitsType(int dxfUnitsType)
    {
      this.dxfUnitsType = dxfUnitsType;
    }

    [When(@"I request Add File Notification")]
    public void WhenIRequestAddFileNotification()
    {
      MakeUrl();
      addFileNotificationRequester = new Getter<AddFileResult>(this.url);
      addFileNotificationRequester.DoValidRequest();
    }

    [When(@"I request Delete File Notification")]
    public void WhenIRequestDeleteFileNotification()
    {
      MakeUrl();
      deleteFileNotificationRequester = new Getter<RequestResult>(this.url);
      deleteFileNotificationRequester.DoValidRequest();
    }

    [Then(@"the Add File Notification result should be")]
    public void ThenTheAddFileNotificationResultShouldBe(string multilineText)
    {
      AddFileResult expected = JsonConvert.DeserializeObject<AddFileResult>(multilineText);
      Assert.AreEqual(expected, addFileNotificationRequester.CurrentResponse);
    }

    [Then(@"the Delete File Notification result should be")]
    public void ThenTheDeleteFileNotificationResultShouldBe(string multilineText)
    {
      RequestResult expected = JsonConvert.DeserializeObject<RequestResult>(multilineText);
      Assert.IsTrue(expected.Code == deleteFileNotificationRequester.CurrentResponse.Code && expected.Message == deleteFileNotificationRequester.CurrentResponse.Message);
    }

    [When(@"I request Delete File Notification Expecting BadRequest")]
    public void WhenIRequestDeleteFileNotificationExpectingBadRequest()
    {
      MakeUrl();
      deleteFileNotificationRequester = new Getter<RequestResult>(this.url);
      deleteFileNotificationRequester.DoInvalidRequest(HttpStatusCode.BadRequest);
    }

    [Then(@"I should get error code (.*) and message ""(.*)""")]
    public void ThenIShouldGetErrorCodeAndMessage(int errorCode, string message)
    {
      Assert.AreEqual(errorCode, deleteFileNotificationRequester.CurrentResponse.Code);
      Assert.AreEqual(message, deleteFileNotificationRequester.CurrentResponse.Message);
    }

    private void MakeUrl()
    {
      this.url = $"{this.url}?projectUid={this.projectUid}&filedescriptor={this.fileDescriptor}&fileId={this.fileId}&fileUid={this.fileUid}&fileType={this.fileTypeId}&dxfUnitsType={this.dxfUnitsType}";
    }
  }
}
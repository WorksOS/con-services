using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using System.Net;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature="FileAccess")]
  public class FileAccessSteps
  {
    private Poster<FileDescriptor, RawFileAccessResult> fileContentRequester;
    private byte[] fileContents;


    [Given(@"the FileAccess service for file contents URI ""(.*)""")]
    public void GivenTheFileAccessServiceForFileContentsURI(string uri)
    {
      fileContentRequester = new Poster<FileDescriptor, RawFileAccessResult>(RaptorClientConfig.FileAccessSvcBaseUri + uri);
    }

    [When(@"I download ""(.*)"" at ""(.*)"" from ""(.*)"" expecting the downloaded file")]
    public void WhenIDownloadAtFromExpectingTheDownloadedFile(string fileName, string path, string filespaceId)
    {
      fileContentRequester.CurrentRequest = new FileDescriptor() { fileName = fileName, path = path, filespaceId = filespaceId };

      //fileContentRequester.DoValidRequest();

      string requestBodyString = JsonConvert.SerializeObject(fileContentRequester.CurrentRequest);

      HttpWebResponse httpResponse = RaptorServicesClientUtil.DoHttpRequest(fileContentRequester.Uri,
           "POST", "application/json", "image/png", requestBodyString);

      fileContents = RaptorServicesClientUtil.GetStreamContentsFromResponse(httpResponse);
    }

    [Then(@"the file contents should be present")]
    public void ThenTheFileContentsShouldBePresent()
    {
      Assert.IsNotNull(fileContents, "No file's contents returned.");

      Assert.IsTrue(fileContents.Length > 0, "File is empty.");
    }

    [When(@"I download ""(.*)"" at ""(.*)"" from ""(.*)"" expecting no downloaded file and BadRequest response")]
    public void WhenIDownloadAtFromExpectingNoDownloadedFileAndBadRequestResponse(string fileName, string path, string filespaceId)
    {
      fileContentRequester.CurrentRequest = new FileDescriptor() { fileName = fileName, path = path, filespaceId = filespaceId };

      fileContentRequester.DoInvalidRequest();
    }

    [Then(@"the response should have Code (.*) and Message ""(.*)"" and no file contents should be present")]
    public void ThenTheResponseShouldHaveCodeAndMessageAndNoFileContentsShouldBePresent(int code, string message)
    {
      Assert.IsTrue(fileContentRequester.CurrentResponse.Code == code && fileContentRequester.CurrentResponse.Message == message,
                string.Format("Expected Code {0} and Message {1}, but got {2} and {3} instead.",
                code, message, fileContentRequester.CurrentResponse.Code, fileContentRequester.CurrentResponse.Message));
    }

  }
}

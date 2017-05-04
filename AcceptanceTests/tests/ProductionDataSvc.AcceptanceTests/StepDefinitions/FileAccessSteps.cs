using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using TechTalk.SpecFlow;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
  [Binding, Scope(Feature="FileAccess")]
  public class FileAccessSteps
  {
    private Poster<FileAccessRequest, DummyRequestResult> fileRequester;
    private Poster<FileDescriptor, RawFileAccessResult> fileContentRequester;

    [Given(@"the FileAccess service URI ""(.*)""")]
    public void GivenTheFileAccessServiceURI(string uri)
    {
      fileRequester = new Poster<FileAccessRequest, DummyRequestResult>(RaptorClientConfig.FileAccessSvcBaseUri + uri);
    }

    [Given(@"""(.*)"" does not already exist")]
    public void GivenDoesNotAlreadyExist(string localPath)
    {
      if (!Directory.Exists("D:\\"))
          ScenarioContext.Current.Pending();

      localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), localPath);
      if(File.Exists(localPath))
      {
        File.Delete(localPath);
        if(File.Exists(localPath))
        {
            ScenarioContext.Current.Pending();
        }
      }
    }

    [Given(@"""(.*)"" already exists")]
    public void GivenAlreadyExists(string localPath)
    {
      if (!Directory.Exists("D:\\"))
          ScenarioContext.Current.Pending();

      localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), localPath);
      if(!File.Exists(localPath))
      {
        File.Create(localPath).Dispose();
        if(!File.Exists(localPath))
        {
            ScenarioContext.Current.Pending();
        }
      }
    }

    [When(@"I download ""(.*)"" at ""(.*)"" from ""(.*)"" to ""(.*)""")]
    public void WhenIDownloadAtFromTo(string fileName, string path, string filespaceId, string localPath)
    {
      localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), localPath);
      fileRequester.CurrentRequest = new FileAccessRequest()
      {
        file = new FileDescriptor() { fileName = fileName, path = path, filespaceId = filespaceId },
        localPath = localPath
      };

      fileRequester.DoValidRequest();
    }

    [When(@"I download ""(.*)"" at ""(.*)"" from ""(.*)"" to ""(.*)"" expecting BadRequest response")]
    public void WhenIDownloadAtFromToExpectingBadRequestResponse(string fileName, string path, string filespaceId, string localPath)
    {
      if(localPath == "Random")
      {
        localPath = string.Format("IDontExist_{0}", DateTime.Now.Ticks);
      }
      localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), localPath);
      fileRequester.CurrentRequest = new FileAccessRequest()
      {
        file = new FileDescriptor() { fileName = fileName, path = path, filespaceId = filespaceId },
        localPath = localPath
      };

      fileRequester.DoInvalidRequest();
    }

    [Then(@"""(.*)"" should be present")]
    public void ThenShouldBePresent(string localPath)
    {
      localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), localPath);
      Assert.IsTrue(File.Exists(localPath), string.Format("Expected file {0} downloaded, but it's not there.", localPath));
    }

    [Then(@"the response should have Code (.*) and Message ""(.*)""")]
    public void ThenTheResponseShouldHaveCodeAndMessage(int code, string message)
    {
      Assert.IsTrue(fileRequester.CurrentResponse.Code == code && fileRequester.CurrentResponse.Message == message,
          string.Format("Expected Code {0} and Message {1}, but got {2} and {3} instead.", 
          code, message, fileRequester.CurrentResponse.Code, fileRequester.CurrentResponse.Message));
    }

    [Given(@"the FileAccess service for file content URI ""(.*)""")]
    public void GivenTheFileAccessServiceForFileContentURI(string uri)
    {
      fileContentRequester = new Poster<FileDescriptor, RawFileAccessResult>(RaptorClientConfig.FileAccessSvcBaseUri + uri);
    }

    [When(@"I download ""(.*)"" at ""(.*)"" from ""(.*)"" expecting the downloaded file")]
    public void WhenIDownloadAtFromExpectingTheDownloadedFile(string fileName, string path, string filespaceId)
    {
      fileContentRequester.CurrentRequest = new FileDescriptor() { fileName = fileName, path = path, filespaceId = filespaceId };

      fileContentRequester.DoValidRequest();
    }

    [Then(@"the response should have Code (.*) and Message ""(.*)"" and the file contents should be present")]
    public void ThenTheResponseShouldHaveCodeAndMessageAndTheFileContentsShouldBePresent(int code, string message)
    {
      Assert.IsTrue(fileContentRequester.CurrentResponse.Code == code && fileContentRequester.CurrentResponse.Message == message,
          string.Format("Expected Code {0} and Message {1}, but got {2} and {3} instead.",
          code, message, fileContentRequester.CurrentResponse.Code, fileContentRequester.CurrentResponse.Message));

      Assert.IsNotNull(fileContentRequester.CurrentResponse.fileContents, "No file's contents returned.");

      Assert.IsTrue(fileContentRequester.CurrentResponse.fileContents.Length > 0, "File is empty.");
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

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

        [Given(@"the FileAccess service URI ""(.*)""")]
        public void GivenTheFileAccessServiceURI(string uri)
        {
            fileRequester = new Poster<FileAccessRequest, DummyRequestResult>(RaptorClientConfig.ProjectSvcBaseUri + uri);
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
    }
}

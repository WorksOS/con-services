using System;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;
using ProductionDataSvc.AcceptanceTests.Models;
using RaptorSvcAcceptTestsCommon.Utils;
using RaptorSvcAcceptTestsCommon.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestAPICoreTestFramework.Utils.Common;

namespace ProductionDataSvc.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "DesignCache")]
    public class DesignCacheSteps
    {
    //private Poster<DesignNameRequest, DummyRequestResult> designCacheDeleter;
    //private ServiceResponse response;

    //[Given(@"the design cache file delete uri ""(.*)"", a project (.*) and a file ""(.*)""")]
    //public void GivenTheDesignCacheFileDeleteUriAProjectAndAFile(string uri, long projectID, string designName)
    //{
    //  designCacheDeleter = new Poster<DesignNameRequest, DummyRequestResult>(
    //      RaptorClientConfig.ProdSvcBaseUri + uri,
    //      new DesignNameRequest() { ProjectId = projectID, DesignFilename = designName });
    //}

    //[Given(@"the following Summary Volumes request is sent to ""(.*)""")]
    //public void GivenTheFollowingSummaryVolumesRequestIsSentTo(string sVuri, string svRequestStr)
    //{
    //    response = RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ReportSvcBaseUri + sVuri,
    //        "POST", RestClientConfig.JsonMediaType, svRequestStr);
    //}

    //[Given(@"the response code is OK (.*)")]
    //public void GivenTheResponseCodeIsOK(int code)
    //{
    //    if (response.HttpCode != (HttpStatusCode)code)
    //    {
    //        ScenarioContext.Current.Pending();
    //    }
    //}

    //[When(@"I delete this file")]
    //public void WhenIDeleteThisFile()
    //{
    //    designCacheDeleter.DoValidRequest();
    //}

    //[When(@"the following Summary Volumes request is sent to ""(.*)""")]
    //public void WhenTheFollowingSummaryVolumesRequestIsSentTo(string sVuri, string svRequestStr)
    //{
    //    response = RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ReportSvcBaseUri + sVuri,
    //        "POST", RestClientConfig.JsonMediaType, svRequestStr);
    //}

    //[Then(@"the response code should be BadRequest (.*)")]
    //public void ThenTheResponseCodeShouldBeBadRequest(int code)
    //{
    //    Assert.AreEqual((HttpStatusCode)code, response.HttpCode);
    //}

    private Poster<DesignNameRequest, DummyRequestResult> designCacheDeleter;

        [Given(@"the DeleteDesignCacheFile service URI ""(.*)"", a project (.*) and a file named ""(.*)""")]
        public void GivenTheDeleteDesignCacheFileServiceURIAProjectAndAFileNamed(string uri, long projectID, string designName)
        {
            if (!Directory.Exists("\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache"))
                ScenarioContext.Current.Pending();

            designCacheDeleter = new Poster<DesignNameRequest, DummyRequestResult>(
                RaptorClientConfig.ProdSvcBaseUri + uri,
                new DesignNameRequest() { ProjectId = projectID, DesignFilename = designName });
        }

        [Given(@"the following Summary Volumes request is sent to ""(.*)"" to make sure the design file is downloaded if required")]
        public void GivenTheFollowingSummaryVolumesRequestIsSentToToMakeSureTheDesignFileIsDownloadedIfRequired(string sVuri, string svRequestStr)
        {
            string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
            string fullDesignFileCachePath = Path.Combine(designFileCachePath,
                designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

            if (!File.Exists(fullDesignFileCachePath))
            {
                try
                {
                    RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ReportSvcBaseUri + sVuri,
                        "POST", RestClientConfig.JsonMediaType, svRequestStr);
                }
                catch (Exception e)
                {
                    Logger.Error(e, Logger.ContentType.Error);
                }
                finally
                {
                    if (!File.Exists(fullDesignFileCachePath))
                    {
                        Logger.Error(string.Format("Design file {0} doesn't exist and can't download it, so give up deleting test.", fullDesignFileCachePath),
                            Logger.ContentType.Error);
                        ScenarioContext.Current.Pending();
                    }
                }
            }
        }

        [When(@"I delete this file")]
        public void WhenIDeleteThisFile()
        {
            designCacheDeleter.DoValidRequest();
        }

        [Then(@"the file should no longer exist in the design cache")]
        public void ThenTheFileShouldNoLongerExistInTheDesignCache()
        {
            string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
            string fullDesignFileCachePath = Path.Combine(designFileCachePath,
                designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

            Assert.IsFalse(File.Exists(fullDesignFileCachePath), string.Format("Expected file {0} deleted, but it's still there.", fullDesignFileCachePath));
        }

        [Given(@"the file does not already exist in the design cache")]
        public void GivenTheFileDoesNotAlreadyExistInTheDesignCache()
        {
            string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
            string fullDesignFileCachePath = Path.Combine(designFileCachePath,
                designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

            if(File.Exists(fullDesignFileCachePath))
            {
                try
                {
                    designCacheDeleter.DoValidRequest();
                }
                catch (Exception e)
                {
                    Logger.Error(e, Logger.ContentType.Error);
                }
                finally
                {
                    if (File.Exists(fullDesignFileCachePath))
                    {
                        Logger.Error(string.Format("Can't delete design file {0}, so give up downloading test.", fullDesignFileCachePath),
                            Logger.ContentType.Error);
                        ScenarioContext.Current.Pending();
                    }
                }
            }
        }

        [When(@"the following Summary Volumes request is sent to ""(.*)""")]
        public void WhenTheFollowingSummaryVolumesRequestIsSentTo(string sVuri, string svRequestStr)
        {
            try
            {
                RaptorServicesClientUtil.DoHttpRequest(RaptorClientConfig.ReportSvcBaseUri + sVuri,
                    "POST", RestClientConfig.JsonMediaType, svRequestStr);
            }
            catch (Exception e)
            {
                Logger.Error(e, Logger.ContentType.Error);
            }
        }

        [Then(@"the file should be automatically downloaded into the design cache")]
        public void ThenTheFileShouldBeAutomaticallyDownloadedIntoTheDesignCache()
        {
            string designFileCachePath = "\\dev-iolv01.vssengg.com\\ProductionData\\DesignFileCache";
            string fullDesignFileCachePath = Path.Combine(designFileCachePath,
                designCacheDeleter.CurrentRequest.ProjectId.ToString(), designCacheDeleter.CurrentRequest.DesignFilename);

            Assert.IsTrue(File.Exists(fullDesignFileCachePath), string.Format("Expected file {0} downloaded, but it's not there.", fullDesignFileCachePath));
        }
    }
}

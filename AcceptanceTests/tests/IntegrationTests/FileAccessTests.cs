using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Newtonsoft.Json;
using TestUtility;
using VSS.Raptor.Service.Common.Models;


namespace IntegrationTests
{
    [TestClass]
    public class FileAccessTests
    {
        [TestMethod]
        void CanGetFileFromTCC()
        {
            var configuration = new TestConfig();
            var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
                "/77561/1158", "Large Sites Road - Trimble Road.ttm");
            var request = new RestClientUtil();
            request.DoHttpRequest(configuration.webApiUri, "POST", JsonConvert.SerializeObject(requestModel));
        }

    /*    [TestMethod]
        void FailToGetnonExistentFile()
        {
            var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
                "/77561/1158", "IDontExist.ttm");
            var request = new RestClientUtil();

        }*/


    }
}

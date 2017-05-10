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
        public void CanGetFileFromTCC()
        {
            var configuration = new TestConfig();
            var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
                "/77561/1158", "Large Sites Road - Trimble Road.ttm");
            var request = new RestClientUtil();
            var (success,result) = request.DoHttpRequest(configuration.webApiUri, "POST", JsonConvert.SerializeObject(requestModel));
            Assert.IsTrue(!String.IsNullOrEmpty(result));
            Assert.IsTrue(success);
        }

        /*[TestMethod]
        void FailToGetnonExistentFile()
        {
            var configuration = new TestConfig();
            var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
                "/77561/1158", "IDontExist.ttm");
            var request = new RestClientUtil();
            var result = request.DoHttpRequest(configuration.webApiUri, "POST", JsonConvert.SerializeObject(requestModel),HSC);
            Assert.IsTrue(!String.IsNullOrEmpty(result));

        }*/


    }
}

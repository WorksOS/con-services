using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;

namespace IntegrationTests
{
  [TestClass]
  public class FileAccessTests
  {
    [TestMethod]
    public void CanGetFileFromTcc()
    {
      var configuration = new TestConfig();
      var requestModel = FileDescriptor.CreateFileDescriptor("5u8472cda0-9f59-41c9-a5e2-e19f922f91d8", "/77561/1158", "Large Sites Road - Trimble Road.ttm");
      var request = new RestClientUtil();

      (bool success, string result) = request.DoHttpRequest(configuration.webApiUri + "api/v1/rawfiles", "POST", JsonConvert.SerializeObject(requestModel));

      Assert.IsTrue(!string.IsNullOrEmpty(result));
      Assert.IsTrue(success);
    }

    [TestMethod]
    public void FailToGetnonExistentFile()
    {
      var configuration = new TestConfig();
      var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "/77561/1158", "IDontExist.ttm");
      var request = new RestClientUtil();
      (bool success, string result) = request.DoHttpRequest(configuration.webApiUri + "api/v1/rawfiles", "POST", JsonConvert.SerializeObject(requestModel));

      Assert.IsFalse(success);
      Assert.IsTrue(!string.IsNullOrEmpty(result));
      var response = JsonConvert.DeserializeObject< ContractExecutionResult>(result);
      Assert.AreEqual(-3, response.Code);
      Assert.AreEqual("Failed to download file from TCC", response.Message);
    }
  }
}

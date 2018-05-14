using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.Productivity3D.FileAccess.Service.Common.Models;

namespace IntegrationTests
{
  [TestClass]
  public class FileAccessTests
  {
    [TestMethod]
    public void CanGetFileFromTcc()
    {
      var configuration = new TestConfig();
      var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
          "/77561/1158", "Large Sites Road - Trimble Road.ttm");
      var request = new RestClientUtil();
      var (success, result) = request.DoHttpRequest(configuration.webApiUri, "POST", JsonConvert.SerializeObject(requestModel));
      Assert.IsTrue(!string.IsNullOrEmpty(result));
      Assert.IsTrue(success);
    }

    [TestMethod]
    public void FailToGetnonExistentFile()
    {
      var configuration = new TestConfig();
      var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
          "/77561/1158", "IDontExist.ttm");
      var request = new RestClientUtil();
      var (success, _) = request.DoHttpRequest(configuration.webApiUri, "POST", JsonConvert.SerializeObject(requestModel));
      Assert.IsFalse(success);
    }
  }
}

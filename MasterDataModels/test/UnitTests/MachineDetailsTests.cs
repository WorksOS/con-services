using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class MachineDetailsTests
  {
    [TestMethod]
    public void AssetId_Should_serialize_to_string()
    {
      var machineDetails = MachineDetails.Create(123, "machine name", isJohnDoe: false);
      var jsonResult = JsonConvert.SerializeObject(machineDetails);

      Assert.AreEqual("{\"assetID\":\"123\",\"machineName\":\"machine name\",\"isJohnDoe\":false}", jsonResult);
    }
  }
}

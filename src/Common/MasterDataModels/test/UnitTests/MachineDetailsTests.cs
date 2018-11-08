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


    [TestMethod]
    [DataRow("{\"assetID\":123,\"machineName\":\"machine name\",\"isJohnDoe\":false}")]
    [DataRow("{\"assetID\":\"123\",\"machineName\":\"machine name\",\"isJohnDoe\":false}")]
    public void AssetId_Should_deserialize_to_long(string json)
    {
      var machineDetails = JsonConvert.DeserializeObject<MachineDetails>(json);

      Assert.AreEqual(123, machineDetails.AssetId);
    }
  }
}

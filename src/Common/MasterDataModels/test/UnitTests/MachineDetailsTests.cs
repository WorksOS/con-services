using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  public class MachineDetailsTests
  {
    [Fact]
    public void AssetId_Should_serialize_to_string()
    {
      var machineDetails = new MachineDetails(123, "machine name", isJohnDoe: false);
      var jsonResult = JsonConvert.SerializeObject(machineDetails);

      Assert.Equal("{\"assetID\":\"123\",\"machineName\":\"machine name\",\"isJohnDoe\":false,\"assetUid\":null}", jsonResult);
    }
    
    [Theory]
    [InlineData("{\"assetID\":123,\"machineName\":\"machine name\",\"isJohnDoe\":false}")]
    [InlineData("{\"assetID\":\"123\",\"machineName\":\"machine name\",\"isJohnDoe\":false}")]
    public void AssetId_Should_deserialize_to_long(string json)
    {
      var machineDetails = JsonConvert.DeserializeObject<MachineDetails>(json);

      Assert.Equal(123, machineDetails.AssetId);
    }
  }
}

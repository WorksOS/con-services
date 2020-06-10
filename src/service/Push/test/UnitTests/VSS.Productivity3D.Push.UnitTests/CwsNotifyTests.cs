using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Push.Models;
using Xunit;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class CwsNotifyTests
  {
    [Fact]
    public void ParseModel()
    {
      var data = @" {""accountTrn"":""trn::profilex:us-west-2:account:94885d66-6f8a-4b4a-af3d-d377a720b651"",""projectTrn"":""trn::profilex:us-west-2:project:f7ad2bc5-eeed-4873-abb1-d6d2728e2393"",""updatedTrns"":[""trn::profilex:us-west-2:device:08d7c716-a843-68b0-01d9-bf0001000c12""],""updateType"":3}";

      var model = JsonConvert.DeserializeObject<CwsTrnUpdate>(data);

      Assert.NotNull(model);
      Assert.Equal("trn::profilex:us-west-2:account:94885d66-6f8a-4b4a-af3d-d377a720b651", model.AccountTrn);
      Assert.Equal("trn::profilex:us-west-2:project:f7ad2bc5-eeed-4873-abb1-d6d2728e2393", model.ProjectTrn);
      Assert.Contains("trn::profilex:us-west-2:device:08d7c716-a843-68b0-01d9-bf0001000c12", model.UpdatedTrns);
      Assert.Equal(3, model.UpdateType);
    }
  }
}

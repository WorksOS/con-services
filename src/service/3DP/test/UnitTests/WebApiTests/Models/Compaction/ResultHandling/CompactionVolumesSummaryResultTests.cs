using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Models.Compaction.ResultHandling
{
  [TestClass]
  public class CompactionVolumesSummaryResultTests
  {
    [TestMethod]
    public void TestJsonConvert()
    {
      const string data = @"{""boundingExtents"":{""maxX"":678016.7800000001,""maxY"":4053787.02,""maxZ"":1E+308,""minX"":-402132.28,""minY"":1125.0600000000002,""minZ"":1E+308},""cut"":982.902102368398,""fill"":632.3848257568771,""cutArea"":4649.0852,""fillArea"":3724.8632000000007,""totalCoverageArea"":10767.677600000003,""code"":0,""message"":""success""}";
      var model = JsonConvert.DeserializeObject<SummaryVolumesResult>(data);

      Assert.IsNotNull(model);
      Assert.AreEqual(model.Cut, 982.902102368398d);
      Assert.AreEqual(model.Fill, 632.3848257568771d);
      Assert.AreEqual(model.CutArea, 4649.0852d);
      Assert.AreEqual(model.FillArea, 3724.8632000000007d);
      Assert.AreEqual(model.TotalCoverageArea, 10767.677600000003d);
    }

  }
}

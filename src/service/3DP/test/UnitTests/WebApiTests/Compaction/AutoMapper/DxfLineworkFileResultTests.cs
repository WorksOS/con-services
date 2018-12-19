using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.AutoMapper
{
  [TestClass]
  public class DxfLineworkFileResultTests
  {
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void MapDxfLineworkFileResultToGeoJson()
    {
      var ps = new DxfLineworkFileResult();


      var cmv = AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
    }
  }
}

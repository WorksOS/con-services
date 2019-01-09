using ASNodeDecls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLPDDecls;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;

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
      var fencePoints = new [] { TWGS84Point.Point(3.4, 4.5) };
      var boundary = new TWGS84FenceContainer { FencePoints  = fencePoints };
      TWGS84LineworkBoundary[] boundaries = { new TWGS84LineworkBoundary { Boundary = boundary } };
      
      var dxfLineworkFileResult = new DxfLineworkFileResult(TASNodeErrorStatus.asneOK, "Success", boundaries);
      
      var geomentryMappingResult = AutoMapperUtility.Automapper.Map<Geometry>(dxfLineworkFileResult.LineworkBoundaries[0].Boundary);
    }
  }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Models
{
  [TestClass]
  public class TagFileTests
  {
    [TestMethod]
    public void CanCreateTagFileTest()
    {
      var validator = new DataAnnotationsValidator();
      byte[] data = { 0x1, 0x2, 0x3 };

      WGSPoint[] points = {
        new WGSPoint(0.631986074660308, -2.00757760231466),
        new WGSPoint(0.631907507374149, -2.00758733949739),
        new WGSPoint(0.631904485465203, -2.00744352879854),
        new WGSPoint(0.631987283352491, -2.00743753668608)
      };

      var fence = new WGS84Fence(points);
      var tagfile = TagFileRequestLegacy.CreateTagFile("test.dxf", data, 10, fence, 11, false, false);

      Assert.IsTrue(validator.TryValidate(tagfile, out _));

      tagfile = TagFileRequestLegacy.CreateTagFile("te$#@#%%&^%&^%#G<>SFDGREYT*st.dxf", data, 10, null, 11, false, false);

      Assert.IsFalse(validator.TryValidate(tagfile, out _));
    }
  }
}

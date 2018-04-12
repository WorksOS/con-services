using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Models
{
  [TestClass]
    public class TAGFileTests
    {
      [TestMethod]
        public void CanCreateTagFileTest()
        {

            var validator = new DataAnnotationsValidator();
            byte[] data = new byte[] {0x1, 0x2, 0x3};
          List<WGSPoint> points = new List<WGSPoint>
          {
            WGSPoint.CreatePoint(0.631986074660308, -2.00757760231466),
            WGSPoint.CreatePoint(0.631907507374149, -2.00758733949739),
            WGSPoint.CreatePoint(0.631904485465203, -2.00744352879854),
            WGSPoint.CreatePoint(0.631987283352491, -2.00743753668608)
          };
          WGS84Fence fence = WGS84Fence.CreateWGS84Fence(points.ToArray());
            TagFileRequest tagfile = TagFileRequest.CreateTagFile("test.dxf", data, 10, fence,
                    11, false, false);
          Assert.IsTrue(validator.TryValidate(tagfile, out ICollection<ValidationResult> results));


            /*tagfile = TAGFile.CreateTagFile("test.dxf", data, 10, null,
                    11, false, false);
            Assert.IsFalse(validator.TryValidate(tagfile, out results));*/

            tagfile = TagFileRequest.CreateTagFile("te$#@#%%&^%&^%#G<>SFDGREYT*st.dxf", data, 10, null,
                11, false, false);
            Assert.IsFalse(validator.TryValidate(tagfile, out results));
        }

    }
}

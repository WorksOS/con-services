using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.FIlters;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
    [TestClass]
    public class ValidationAttributeTests
    {
        [TestMethod]
        public void DecimalIsWithinRangeAttributeTest()
        {
            DecimalIsWithinRangeAttribute attribute = new DecimalIsWithinRangeAttribute(-10, 10);
            Assert.IsTrue(attribute.IsValid(2));
            Assert.IsFalse(attribute.IsValid(20));
        }

        [TestMethod]
        public void IntIsWithinRangeAttributeTest()
        {
            IntIsWithinRangeAttribute attribute = new IntIsWithinRangeAttribute(-10, 10);
            Assert.IsTrue(attribute.IsValid(2));
            Assert.IsFalse(attribute.IsValid(20));
        }

        [TestMethod]
        public void MoreThanTwoPointsAttributeTest()
        {
            MoreThanTwoPointsAttribute attribute = new MoreThanTwoPointsAttribute();
            List<WGSPoint3D> list = new List<WGSPoint3D>();
            for (int i=0;i<5;i++)
                list.Add(new WGSPoint3D(3,3));
            List<WGSPoint3D> list2 = new List<WGSPoint3D>();
            for (int i = 0; i < 55; i++)
                list2.Add(new WGSPoint3D(3, 3));
            List<WGSPoint3D> list3 = new List<WGSPoint3D>();
            for (int i = 0; i < 1; i++)
                list3.Add(new WGSPoint3D(3, 3));

            Assert.IsTrue(attribute.IsValid(list.ToArray()));
            Assert.IsFalse(attribute.IsValid(list2.ToArray()));
            Assert.IsFalse(attribute.IsValid(list3.ToArray()));

        }

        [TestMethod]
        public void ValidFilenameAttributeTest()
        {
            ValidFilenameAttribute attribute = new ValidFilenameAttribute(16);
            const string validFileName = "c:\\test\\test.txt";
            const string invalidFileName = "c:\\te%@$#s><t**est.>txt";
            const string longinvalidFileName = "c:\\te%@$#gfdsgfdhytueytjuegrhrthjetrgshstest.>txt";
            Assert.IsTrue(attribute.IsValid(validFileName));
            Assert.IsFalse(attribute.IsValid(invalidFileName));
            Assert.IsFalse(attribute.IsValid(longinvalidFileName));

        }
    }
}

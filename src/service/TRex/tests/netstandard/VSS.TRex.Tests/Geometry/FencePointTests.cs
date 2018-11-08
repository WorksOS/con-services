using VSS.TRex.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.TRex.Common;
using Xunit;

namespace VSS.TRex.Tests.Geometry
{
        public class FencePointTests
    {
        [Fact()]
        public void Test_FencePointTests_Creation_WithXY()
        {
            FencePoint fp = new FencePoint(10.0, 20.0);

            Assert.True(fp.X == 10.0 && fp.Y == 20.0 && fp.Z == Consts.NullDouble, "Fence point not created as expected");
        }

        [Fact()]
        public void Test_FencePointTests_Creation_WithXYZ()
        {
            FencePoint fp = new FencePoint(10.0, 20.0, 30.0);

            Assert.True(fp.X == 10.0 && fp.Y == 20.0 && fp.Z == 30.0, "Fence point not created as expected");
        }

        [Fact()]
        public void Test_FencePointTests_Creation_Base()
        {
            FencePoint fp = new FencePoint();

            Assert.True(fp.X == Consts.NullDouble && fp.Y == Consts.NullDouble && fp.Z == Consts.NullDouble, "Fence point not created as expected");
        }

        [Fact()]
        public void Test_FencePointTests_Creation_WithPt()
        {
            FencePoint fp = new FencePoint(10.0, 20.0, 30.0);
            FencePoint fp2 = new FencePoint(fp);

            Assert.True(fp2.X == 10.0 && fp2.Y == 20.0 && fp.Z == 30.0, "Fence point not created as expected");
        }

        [Fact()]
        public void Test_FencePointTests_SetXYTest()
        {
            FencePoint fp = new FencePoint(10.0, 10.0);

            fp.SetXY(100.0, 200.0);

            Assert.True(fp.X == 100.0 && fp.Y == 200.0 && fp.Z == Consts.NullDouble, "Fence point not created as expected");
        }

        [Fact()]
        public void Test_FencePointTests_SetXYZTest()
        {
            FencePoint fp = new FencePoint(10.0, 20.0, 30.0);

            fp.SetXYZ(100.0, 200.0, 300.0);

            Assert.True(fp.X == 100.0 && fp.Y == 200.0 && fp.Z == 300.0, "Fence point not created as expected");
        }

        [Fact()]
        public void Test_FencePointTests_Assign()
        {
            FencePoint fp1 = new FencePoint(10.0, 20.0, 30.0);
            FencePoint fp2 = new FencePoint(100.0, 200.0, 300.0);

            fp1.Assign(fp2);

            Assert.True(fp1.X == 100.0 && fp1.Y == 200.0 && fp1.Z == 300.0, "Fence point not assigned as expected");
        }
    }
}

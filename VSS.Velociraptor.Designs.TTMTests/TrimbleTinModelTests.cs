using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Velociraptor.Designs.TTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.Designs.TTM.Tests
{
    [TestClass()]
    public class TrimbleTinModelTests
    {
        [Ignore()]
        [TestMethod()]
        public void BuildStartPointListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void TrimbleTinModelTest_Creation()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            Assert.IsTrue(TTM != null, "Failed to create TrimbleTinModel instance");
        }

        [Ignore()]
        [TestMethod()]
        public void SetUpSizesTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void ReadTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void WriteTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void WriteDefaultTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void LoadFromFileTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            TTM.LoadFromFile(@"C:\Temp\Bug36372.ttm");

            Assert.IsTrue(TTM.Vertices.Count > 0, "No vertices loaded from TTM file");
            Assert.IsTrue(TTM.Triangles.Count > 0, "No triangles loaded from TTM file");
        }

        [Ignore()]
        [TestMethod()]
        public void SaveToFileTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void SaveToFileTest1()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void BuildEdgeListTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void BuildStartPointListTest1()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void ClearTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsTTMFileTest()
        {
            Assert.IsTrue(TrimbleTINModel.IsTTMFile(@"C:\Temp\Bug36372.ttm", out string error), $"File is not a TTM file when it should be with error='{error}'");
        }

        [Ignore()]
        [TestMethod()]
        public void ReadHeaderFromFileTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [TestMethod()]
        public void GetElevationRangeTest()
        {
            Assert.Fail();
        }
    }
}
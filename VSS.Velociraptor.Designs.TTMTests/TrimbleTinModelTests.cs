using VSS.Velociraptor.Designs.TTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VSS.Velociraptor.Designs.TTM.Tests
{
        public class TrimbleTinModelTests
    {
        [Ignore()]
        [Fact()]
        public void BuildStartPointListTest()
        {
            Assert.Fail();
        }

        [Fact()]
        public void TrimbleTinModelTest_Creation()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            Assert.NotNull(TTM);
        }

        [Ignore()]
        [Fact()]
        public void SetUpSizesTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void ReadTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void WriteTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void WriteDefaultTest()
        {
            Assert.Fail();
        }

        [Fact()]
        public void LoadFromFileTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            TTM.LoadFromFile(@"C:\Temp\Bug36372.ttm");

            Assert.True(TTM.Vertices.Count > 0, "No vertices loaded from TTM file");
            Assert.True(TTM.Triangles.Count > 0, "No triangles loaded from TTM file");
        }

        [Ignore()]
        [Fact()]
        public void SaveToFileTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void SaveToFileTest1()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void BuildEdgeListTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void BuildStartPointListTest1()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void ClearTest()
        {
            Assert.Fail();
        }

        [Fact()]
        public void IsTTMFileTest()
        {
            Assert.True(TrimbleTINModel.IsTTMFile(@"C:\Temp\Bug36372.ttm", out string error), $"File is not a TTM file when it should be with error='{error}'");
        }

        [Ignore()]
        [Fact()]
        public void ReadHeaderFromFileTest()
        {
            Assert.Fail();
        }

        [Ignore()]
        [Fact()]
        public void GetElevationRangeTest()
        {
            Assert.Fail();
        }
    }
}
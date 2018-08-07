﻿using System.IO;
using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
    public class TrimbleTinModelTests
    {
        [Fact(Skip = "not implemented")]
        public void BuildStartPointListTest()
        {
        }

        [Fact()]
        public void TrimbleTinModelTest_Creation()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            Assert.NotNull(TTM);
        }

        [Fact(Skip = "not implemented")]
        public void SetUpSizesTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void ReadTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void WriteTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void WriteDefaultTest()
        {
        }

        [Fact()]
        public void LoadFromFileTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();          

            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

            Assert.True(TTM.Vertices.Count > 0, "No vertices loaded from TTM file");
            Assert.True(TTM.Triangles.Count > 0, "No triangles loaded from TTM file");
        }

        [Fact(Skip = "not implemented")]
        public void SaveToFileTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void SaveToFileTest1()
        {
        }

        [Fact(Skip = "not implemented")]
        public void BuildEdgeListTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void BuildStartPointListTest1()
        {
        }

        [Fact(Skip = "not implemented")]
        public void ClearTest()
        {
        }

        [Fact()]
        public void IsTTMFileTest()
        {
            Assert.True(TrimbleTINModel.IsTTMFile(Path.Combine("TestData", "Bug36372.ttm"), out string error),
                $"File is not a TTM file when it should be with error='{error}'");
        }

        [Fact(Skip = "not implemented")]
        public void ReadHeaderFromFileTest()
        {
        }

        [Fact(Skip = "not implemented")]
        public void GetElevationRangeTest()
        {
        }
    }
}

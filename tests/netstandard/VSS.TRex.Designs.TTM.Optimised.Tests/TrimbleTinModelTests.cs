using System.IO;
using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
    public class TrimbleTinModelTests
    {
        [Fact()]
        public void TrimbleTinModelTest_Creation()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            Assert.NotNull(TTM);
        }

        [Fact(Skip = "not implemented")]
        public void ReadTest()
        {
        }

        [Fact()]
        public void LoadFromFileTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();          

            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

            Assert.True(TTM.Vertices.Items.Length > 0, "No vertices loaded from TTM file");
            Assert.True(TTM.Triangles.Items.Length > 0, "No triangles loaded from TTM file");
        }

        [Fact(Skip = "not implemented")]
        public void ClearTest()
        {
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

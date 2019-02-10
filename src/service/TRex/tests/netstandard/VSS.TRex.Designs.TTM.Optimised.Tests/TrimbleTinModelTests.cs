using System.IO;
using FluentAssertions;
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

        [Fact]
        public void ReadTest()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          byte[] bytes = File.ReadAllBytes(Path.Combine("TestData", "Bug36372.ttm"));

          using (BinaryReader br = new BinaryReader(new MemoryStream(bytes)))
          {
            TTM.Read(br, bytes);
          }

          TTM.Header.NumberOfTriangles.Should().Be(67251);
          TTM.Header.NumberOfVertices.Should().Be(34405);
        }

        [Fact()]
        public void LoadFromFileTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();          

            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

            Assert.True(TTM.Vertices.Items.Length > 0, "No vertices loaded from TTM file");
            Assert.True(TTM.Triangles.Items.Length > 0, "No triangles loaded from TTM file");
        }

        [Fact]
        public void ReadHeaderFromFileTest()
        {
          TTM.TrimbleTINModel TTM = new TTM.TrimbleTINModel();

          TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

          TTM.Header.NumberOfTriangles.Should().Be(67251);
          TTM.Header.NumberOfVertices.Should().Be(34405);
          TTM.Header.MaximumEasting.Should().BeApproximately(248539.6337, 001);
          TTM.Header.MaximumNorthing.Should().BeApproximately(194587.6191, 001);
          TTM.Header.MinimumEasting.Should().BeApproximately(246852.3283, 001);
          TTM.Header.MinimumNorthing.Should().BeApproximately(191674.8496, 001);
        }
    }
}

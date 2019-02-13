using System.IO;
using FluentAssertions;
using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
    public class TrimbleTinModelTests
    {
        [Fact]
        public void BuildStartPointListTest()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

          TTM.StartPoints.Count.Should().Be(16);

          TTM.StartPoints.Clear();
          TTM.StartPoints.Count.Should().Be(0);

          TTM.BuildStartPointList();
          TTM.StartPoints.Count.Should().Be(50);
        }

        [Fact]
        public void TrimbleTinModelTest_Creation()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            Assert.NotNull(TTM);
        }

        [Fact(Skip = "not implemented")]
        public void SetUpSizesTest()
        {
        }

        [Fact]
        public void ReadTest()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          byte[] bytes = File.ReadAllBytes(Path.Combine("TestData", "Bug36372.ttm"));

          using (BinaryReader br = new BinaryReader(new MemoryStream(bytes)))
          {
            TTM.Read(br);
          }

          TTM.Header.NumberOfTriangles.Should().Be(67251);
          TTM.Header.NumberOfVertices.Should().Be(34405);
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

        [Fact]
        public void BuildEdgeListTest()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

          TTM.Edges.Count.Should().Be(1525);

          TTM.Edges.Clear();
          TTM.Edges.Count.Should().Be(0);

          TTM.BuildEdgeList();
          TTM.Edges.Count.Should().Be(1525);
    }

        [Fact]
        public void ClearTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

            TTM.Triangles.Count.Should().Be(67251);
            TTM.Vertices.Count.Should().Be(34405);
            TTM.Edges.Count.Should().Be(1525);
            TTM.StartPoints.Count.Should().Be(16);

            TTM.Clear();

            TTM.Triangles.Count.Should().Be(0);
            TTM.Vertices.Count.Should().Be(0);
            TTM.Edges.Count.Should().Be(0);
            TTM.StartPoints.Count.Should().Be(0);
        }

        [Fact()]
        public void IsTTMFileTest()
        {
            Assert.True(TrimbleTINModel.IsTTMFile(Path.Combine("TestData", "Bug36372.ttm"), out string error),
                $"File is not a TTM file when it should be with error='{error}'");
        }

        [Fact]
        public void ReadHeaderFromFileTest()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();
         
            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));
         
            TTM.Header.NumberOfTriangles.Should().Be(67251);
            TTM.Header.NumberOfVertices.Should().Be(34405);
            TTM.Header.MaximumEasting.Should().BeApproximately(248539.6337, 001);
            TTM.Header.MaximumNorthing.Should().BeApproximately(194587.6191, 001);
            TTM.Header.MinimumEasting.Should().BeApproximately(246852.3283, 001);
            TTM.Header.MinimumNorthing.Should().BeApproximately(191674.8496, 001);
        }

        [Fact]
        public void GetElevationRangeTest()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

          TTM.GetElevationRange(out var MinElev, out var MaxElev);

          MinElev.Should().BeApproximately(22.5, 0.001);
          MaxElev.Should().BeApproximately(37.33, 0.001);
        }
    }
}

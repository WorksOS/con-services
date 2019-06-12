using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using VSS.TRex.Designs.TTM.Exceptions;
using Xunit;

namespace VSS.TRex.Designs.TTM.Tests
{
    public class TrimbleTinModelTests
    {
        private void CheckTTMAttributes(TrimbleTINModel TTM1, TrimbleTINModel TTM2)
        {
          TTM1.Triangles.Count.Should().Be(TTM2.Triangles.Count);
          TTM1.Vertices.Count.Should().Be(TTM2.Vertices.Count);
          TTM1.Edges.Count.Should().Be(TTM2.Edges.Count);
          TTM1.StartPoints.Count.Should().Be(TTM2.StartPoints.Count);
        }

        [Fact]
        public void BuildStartPointList()
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
        public void Creation()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            Assert.NotNull(TTM);
        }

        private void Test_TTMWriteAndReadBack(TrimbleTINModel TTM)
        {
          using (var bw = new BinaryWriter(new MemoryStream(Common.Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION)))
          {
            TTM.Write(bw);

            TrimbleTINModel TTM2 = new TrimbleTINModel();
            MemoryStream ms = bw.BaseStream as MemoryStream;
            ms.Position = 0;
            TTM2.Read(new BinaryReader(ms));

            CheckTTMAttributes(TTM, TTM2);
          }
        }

        [Fact]
        public void Write_Empty()
        {
            Test_TTMWriteAndReadBack(new TrimbleTINModel());
        }

        [Fact]
        public void Write_NonEmpty()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();
           
            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));
            Test_TTMWriteAndReadBack(TTM);
        }

        [Fact]
        public void Read_Empty()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          var fileName = Path.GetTempFileName() + ".ttm";
          TTM.SaveToFile(fileName);

          byte[] bytes = File.ReadAllBytes(fileName);

          using (var br = new BinaryReader(new MemoryStream(bytes)))
          {
            TrimbleTINModel TTM2 = new TrimbleTINModel();
            TTM2.Read(br);

            CheckTTMAttributes(TTM, TTM2);
          }

          File.Delete(fileName);
        }

        [Fact]
        public void Read_NonEmpty()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();
            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));
          
            byte[] bytes = File.ReadAllBytes(Path.Combine("TestData", "Bug36372.ttm"));
          
            using (var br = new BinaryReader(new MemoryStream(bytes)))
            {
                TrimbleTINModel TTM2 = new TrimbleTINModel();
                TTM2.Read(br);
          
                CheckTTMAttributes(TTM, TTM2);
            }
        }

        [Fact]
        public void LoadFromFile()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();          

            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

            Assert.True(TTM.Vertices.Count > 0, "No vertices loaded from TTM file");
            Assert.True(TTM.Triangles.Count > 0, "No triangles loaded from TTM file");
            TTM.Loading.Should().BeFalse();
        }

        [Fact]
        public void LoadFromFile_ModelName()
        {
          var TTM = new TrimbleTINModel();
          TTM.ModelName = "ModelName";

          var fileName = Path.GetTempFileName() + ".ttm";
          TTM.SaveToFile(fileName);

          var TTM2 = new TrimbleTINModel();
          TTM2.LoadFromFile(fileName);
          TTM2.ModelName.Should().Be("ModelName");

          File.Delete(fileName);
        }

        [Fact]
        public void LoadFromFile_NoModelName()
        {
            var TTM = new TrimbleTINModel();
            TTM.ModelName = "";

            var fileName = Path.GetTempFileName() + ".ttm";
            TTM.SaveToFile(fileName);

            var TTM2 = new TrimbleTINModel();
            TTM2.LoadFromFile(fileName);
            TTM2.ModelName.Should().Be(Path.ChangeExtension(Path.GetFileName(fileName), ""));

            File.Delete(fileName);
        }

        [Fact]
        public void SaveToFile_Empty()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();

            var fileName = Path.GetTempFileName() + ".ttm";
            TTM.SaveToFile(fileName);

            File.Delete(fileName);
        }

        [Fact]
        public void SaveToFile_NonEmpty()
        {
            TrimbleTINModel TTM = new TrimbleTINModel();
        
            TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));
        
            var fileName = Path.GetTempFileName() + ".ttm";
            TTM.SaveToFile(fileName);

            File.Delete(fileName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveToFile_WithEdgeBuilding(bool state)
        {
          TrimbleTINModel TTM = new TrimbleTINModel();
          TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

          var fileName = Path.GetTempFileName() + ".ttm";
          TTM.SaveToFile(fileName, state);
          File.Delete(fileName);
        }

        [Fact]
        public void BuildStartPoint_Empty()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          TTM.BuildStartPointList();

          TTM.StartPoints.Count.Should().Be(0);
        }

        [Fact]
        public void BuildEdgeList()
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
        public void Clear()
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

        [Fact]
        public void ReadHeaderFromFile()
        {
            TrimbleTINModel.ReadHeaderFromFile(Path.Combine("TestData", "Bug36372.ttm"), out var header).Should().BeTrue();
         
            header.NumberOfTriangles.Should().Be(67251);
            header.NumberOfVertices.Should().Be(34405);
            header.MaximumEasting.Should().BeApproximately(248539.6337, 001);
            header.MaximumNorthing.Should().BeApproximately(194587.6191, 001);
            header.MinimumEasting.Should().BeApproximately(246852.3283, 001);
            header.MinimumNorthing.Should().BeApproximately(191674.8496, 001);
        }

        [Fact]
        public void ReadHeaderFromFile_InvalidFile()
        {
          var fileName = Path.GetTempFileName() + ".ttm";
          File.WriteAllBytes(fileName, new byte[100]);

          TrimbleTINModel.ReadHeaderFromFile(fileName, out _).Should().BeFalse();

          File.Delete(fileName);
        }

        [Fact]
        public void GetElevationRange()
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          TTM.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

          TTM.GetElevationRange(out var MinElev, out var MaxElev);

          MinElev.Should().BeApproximately(22.5, 0.001);
          MaxElev.Should().BeApproximately(37.33, 0.001);
        }

        [Theory]
        [InlineData(100, 100, 0.0)]
        [InlineData(200, 100, 0.0)]
        [InlineData(100, 200, 0.0)]
        public void SaveToFile_SmallTTM(double eastSize, double northSize, double elevation)
        {
          TrimbleTINModel TTM = new TrimbleTINModel();

          TTM.Vertices.InitPointSearch(-1, -1, eastSize + 1, northSize + 1, 100);

          TTM.Triangles.AddTriangle(TTM.Vertices.AddPoint(0, 0, elevation), 
                                    TTM.Vertices.AddPoint(0, northSize, elevation), 
                                    TTM.Vertices.AddPoint(eastSize, 0, elevation));
          TTM.Triangles.AddTriangle(TTM.Vertices.AddPoint(eastSize, 0, elevation), 
                                    TTM.Vertices.AddPoint(eastSize, northSize, elevation), 
                                    TTM.Vertices.AddPoint(0, northSize, elevation));

           var fileName = Path.GetTempFileName() + ".ttm";
           TTM.SaveToFile(fileName, 0.001, 0.001);

           TrimbleTINModel TTM2 = new TrimbleTINModel();
           TTM2.LoadFromFile(fileName);

           CheckTTMAttributes(TTM, TTM2);

           File.Delete(fileName);
        }

        [Fact]
        public void ReadInvalidTTMFile_ErrorReadingHeader()
        {
           var TTM = new TrimbleTINModel();

           var fileName = Path.GetTempFileName() + ".ttm";
           File.WriteAllBytes(fileName, new byte[100]);

           Action act = () => TTM.LoadFromFile(fileName);

           act.Should().Throw<TTMFileReadException>().WithMessage("Exception at TTM loading phase Error reading header");

           File.Delete(fileName);
        }

        [Theory]
        [InlineData(Consts.TTMMajorVersion + 1, Consts.TTMMinorVersion)]
        [InlineData(Consts.TTMMajorVersion, Consts.TTMMinorVersion + 1)]
        public void ReadInvalidTTMFile_ErrorVerifyingVersion(byte majorVersion, byte minorVersion)
        {
          var TTM = new TrimbleTINModel();
          TTM.Header.FileMajorVersion = majorVersion;
          TTM.Header.FileMinorVersion = minorVersion;

          var fileName = Path.GetTempFileName() + ".ttm";
          TTM.SaveToFile(fileName);

          // Pervert the version in the file. Byte 1 = major version, byte 2 = minor version

          var bytes = File.ReadAllBytes(fileName);
          bytes[0] = majorVersion;
          bytes[1] = minorVersion;

          File.WriteAllBytes(fileName, bytes);

          Action act = () => TTM.LoadFromFile(fileName);

          act.Should().Throw<TTMFileReadException>().WithMessage("*Unable to read this version*");

          File.Delete(fileName);
        }

        [Fact]
        public void ReadInvalidTTMFile_ErrorVerifyingTTMIdentifier()
        {
          var TTM = new TrimbleTINModel();

          var fileName = Path.GetTempFileName() + ".ttm";
          File.WriteAllBytes(fileName, new byte[500]);

          Action act = () => TTM.LoadFromFile(fileName);

          act.Should().Throw<TTMFileReadException>().WithMessage("File is not a Trimble TIN Model.");

          File.Delete(fileName);
        }

        [Fact]
        public void IsTTMFile_ErrorReadingHeader()
        {
          var fileName = Path.GetTempFileName() + ".ttm";
          File.WriteAllBytes(fileName, new byte[100]);

          TrimbleTINModel.IsTTMFile(fileName, out var message).Should().BeFalse();
          message.Should().StartWith("Error reading header");

          File.Delete(fileName);
        }

        [Fact]
        public void IsTTMFile_ErrorVerifyingTTMIdentifier_Null()
        {
          var fileName = Path.GetTempFileName() + ".ttm";
          File.WriteAllBytes(fileName, new byte[500]);

          TrimbleTINModel.IsTTMFile(fileName, out var message).Should().BeFalse();
          message.Should().StartWith("File is not a Trimble TIN Model");

          File.Delete(fileName);
        }

        [Fact]
        public void IsTTMFile_ErrorVerifyingTTMIdentifier_NotNull()
        {                                       
         const string InvalidTTMFileIdentifier = "NOT A VALID TTM\0\0\0\0\0"; 

          var TTM = new TrimbleTINModel();
          TTM.Header.FileSignature = ASCIIEncoding.ASCII.GetBytes(InvalidTTMFileIdentifier);

          var fileName = Path.GetTempFileName() + ".ttm";
          TTM.SaveToFile(fileName);

          TrimbleTINModel.IsTTMFile(fileName, out var message).Should().BeFalse();
          message.Should().StartWith("File is not a Trimble TIN Model");

          File.Delete(fileName);
        }

        [Theory]
        [InlineData(Consts.TTMMajorVersion + 1, Consts.TTMMinorVersion)]
        [InlineData(Consts.TTMMajorVersion, Consts.TTMMinorVersion + 1)]
        public void IsTTMFile_ErrorVerifyingVersion(byte majorVersion, byte minorVersion)
        {
          var TTM = new TrimbleTINModel();
          TTM.Header.FileMajorVersion = majorVersion;
          TTM.Header.FileMinorVersion = minorVersion;

          var fileName = Path.GetTempFileName() + ".ttm";
          TTM.SaveToFile(fileName);

          // Pervert the version in the file. Byte 1 = major version, byte 2 = minor version

          var bytes = File.ReadAllBytes(fileName);
          bytes[0] = majorVersion;
          bytes[1] = minorVersion;

          File.WriteAllBytes(fileName, bytes);

          TrimbleTINModel.IsTTMFile(fileName, out var message).Should().BeFalse();
          message.Should().StartWith("TTM.IsTTMFile(): Unable to read this version");

          File.Delete(fileName);
        }

        [Fact]
        public void ReadInvalidTTMFile_IsTTMFile_Success()
        {
          TrimbleTINModel.IsTTMFile(Path.Combine("TestData", "Bug36372.ttm"), out _).Should().BeTrue();
        }
    }
}

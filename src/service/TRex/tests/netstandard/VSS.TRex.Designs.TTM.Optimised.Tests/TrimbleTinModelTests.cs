using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;
using Xunit;

namespace VSS.TRex.Designs.TTM.Optimised.Tests
{
  public class TrimbleTinModelTests
  {
    private void CheckTTMAttributes(VSS.TRex.Designs.TTM.TrimbleTINModel ttm1, VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel ttm2)
    {
      ttm1.Triangles.Count.Should().Be(ttm2.Triangles.Items.Length);
      ttm1.Vertices.Count.Should().Be(ttm2.Vertices.Items.Length);
      ttm1.Edges.Count.Should().Be(ttm2.Edges.Items.Length);
      ttm1.StartPoints.Count.Should().Be(ttm2.StartPoints.Items.Length);
    }

    private void CheckTTMAttributes(TrimbleTINModel ttm1, TrimbleTINModel ttm2)
    {
      ttm1.Triangles.Items.Length.Should().Be(ttm2.Triangles.Items.Length);
      ttm1.Vertices.Items.Length.Should().Be(ttm2.Vertices.Items.Length);
      ttm1.Edges.Items.Length.Should().Be(ttm2.Edges.Items.Length);
      ttm1.StartPoints.Items.Length.Should().Be(ttm2.StartPoints.Items.Length);
    }

    [Fact]
    public void TrimbleTinModelTest_Creation()
    {
      var ttm = new TrimbleTINModel();

      Assert.NotNull(ttm);
    }

    [Fact]
    public void Read_NonEmpty()
    {
      var ttm = new TrimbleTINModel();

      byte[] bytes = File.ReadAllBytes(Path.Combine("TestData", "Bug36372.ttm"));

      using (BinaryReader br = new BinaryReader(new MemoryStream(bytes)))
      {
        ttm.Read(br, bytes);
      }

      ttm.Header.NumberOfTriangles.Should().Be(67251);
      ttm.Header.NumberOfVertices.Should().Be(34405);
    }

    [Fact]
    public void Read_Empty()
    {
      var ttm = new VSS.TRex.Designs.TTM.TrimbleTINModel();

      var fileName = Path.GetTempFileName() + ".ttm";
      ttm.SaveToFile(fileName);

      byte[] bytes = File.ReadAllBytes(fileName);

      using (var br = new BinaryReader(new MemoryStream(bytes)))
      {
        var ttm2 = new TrimbleTINModel();
        ttm2.Read(br, bytes);

        CheckTTMAttributes(ttm, ttm2);
      }

      File.Delete(fileName);
    }

    [Fact()]
    public void LoadFromFile()
    {
      var ttm = new TrimbleTINModel();

      ttm.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));

      Assert.True(ttm.Vertices.Items.Length > 0, "No vertices loaded from TTM file");
      Assert.True(ttm.Triangles.Items.Length > 0, "No triangles loaded from TTM file");
    }

    [Fact]
    public void LoadFromFile_ModelName()
    {
      var ttm = new TTM.TrimbleTINModel();
      ttm.ModelName = "ModelName";

      var fileName = Path.GetTempFileName() + ".ttm";
      ttm.SaveToFile(fileName);

      var ttm2 = new TrimbleTINModel();
      ttm2.LoadFromFile(fileName);
      ttm2.ModelName.Should().Be("ModelName");

      File.Delete(fileName);
    }

    [Fact]
    public void LoadFromFile_NoModelName()
    {
      var ttm = new TTM.TrimbleTINModel();
      ttm.ModelName = "";

      var fileName = Path.GetTempFileName() + ".ttm";
      ttm.SaveToFile(fileName);

      var ttm2 = new TrimbleTINModel();
      ttm2.LoadFromFile(fileName);
      ttm2.ModelName.Should().Be(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)));

      File.Delete(fileName);
    }

    [Fact]
    public void ReadHeaderFromFile()
    {
      var ttm = new TrimbleTINModel();

      ttm.LoadFromFile(Path.Combine("TestData", "Bug36372.ttm"));
      
      ttm.Header.NumberOfTriangles.Should().Be(67251);
      ttm.Header.NumberOfVertices.Should().Be(34405);
      ttm.Header.MaximumEasting.Should().BeApproximately(248539.6337, 001);
      ttm.Header.MaximumNorthing.Should().BeApproximately(194587.6191, 001);
      ttm.Header.MinimumEasting.Should().BeApproximately(246852.3283, 001);
      ttm.Header.MinimumNorthing.Should().BeApproximately(191674.8496, 001);
    }

    [Fact]
    public void ReadInvalidTTMFile_ErrorReadingHeader()
    {
      var fileName = Path.GetTempFileName() + ".ttm";
      File.WriteAllBytes(fileName, new byte[100]);

      var ttm = new TrimbleTINModel();
      Action act = () => ttm.LoadFromFile(fileName);

      act.Should().Throw<TTMFileReadException>().WithMessage("Exception at TTM loading phase Error reading header");

      File.Delete(fileName);
    }

    [Theory]
    [InlineData(Consts.TTM_MAJOR_VERSION + 1, Consts.TTM_MINOR_VERSION)]
    [InlineData(Consts.TTM_MAJOR_VERSION, Consts.TTM_MINOR_VERSION + 1)]
    public void ReadInvalidTTMFile_ErrorVerifyingVersion(byte majorVersion, byte minorVersion)
    {
      var ttm = new TTM.TrimbleTINModel();
      ttm.Header.FileMajorVersion = majorVersion;
      ttm.Header.FileMinorVersion = minorVersion;

      var fileName = Path.GetTempFileName() + ".ttm";
      ttm.SaveToFile(fileName);

      // Pervert the version in the file. Byte 1 = major version, byte 2 = minor version

      var bytes = File.ReadAllBytes(fileName);
      bytes[0] = majorVersion;
      bytes[1] = minorVersion;

      File.WriteAllBytes(fileName, bytes);

      var TTM2 = new TrimbleTINModel();
      Action act = () => TTM2.LoadFromFile(fileName);

      act.Should().Throw<TTMFileReadException>().WithMessage("*Unable to read this version*");

      File.Delete(fileName);
    }

    [Fact]
    public void ReadInvalidTTMFile_ErrorVerifyingTTMIdentifier()
    {
      var ttm = new TrimbleTINModel();

      var fileName = Path.GetTempFileName() + ".ttm";
      File.WriteAllBytes(fileName, new byte[500]);

      Action act = () => ttm.LoadFromFile(fileName);

      act.Should().Throw<TTMFileReadException>().WithMessage("File is not a Trimble TIN Model.");

      File.Delete(fileName);
    }

    [Theory]
    [InlineData(100, 100, 0.0)]
    [InlineData(200, 100, 0.0)]
    [InlineData(100, 200, 0.0)]
    public void LoadFromFile_SmallTTM(double eastSize, double northSize, double elevation)
    {
      var ttm = new TTM.TrimbleTINModel();

      ttm.Vertices.InitPointSearch(-1, -1, eastSize + 1, northSize + 1, 100);

      ttm.Triangles.AddTriangle(ttm.Vertices.AddPoint(0, 0, elevation),
        ttm.Vertices.AddPoint(0, northSize, elevation),
        ttm.Vertices.AddPoint(eastSize, 0, elevation));
      ttm.Triangles.AddTriangle(ttm.Vertices.AddPoint(eastSize, 0, elevation),
        ttm.Vertices.AddPoint(eastSize, northSize, elevation),
        ttm.Vertices.AddPoint(0, northSize, elevation));

      var fileName = Path.GetTempFileName() + ".ttm";
      ttm.SaveToFile(fileName, 0.001, 0.001);

      var ttm2 = new TrimbleTINModel();
      ttm2.LoadFromFile(fileName);

      CheckTTMAttributes(ttm, ttm2);

      File.Delete(fileName);
    }
  }
}

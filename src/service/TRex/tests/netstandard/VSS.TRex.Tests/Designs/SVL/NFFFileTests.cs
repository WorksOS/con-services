using System.IO;
using FluentAssertions;
using VSS.TRex.Designs.SVL;
using Xunit;

namespace VSS.TRex.Tests.Designs.SVL
{
  public class NFFFileTests
  {
    [Fact]
    public void Creation()
    {
      var f = new TNFFFile();
      f.Should().NotBeNull();
    }

    [Theory]
    [InlineData(TNFFFileType.nffSVDFile)]
    [InlineData(TNFFFileType.nffSVLFile)]
    public void Creation2(TNFFFileType fileType)
    {
      var f = new TNFFFile(fileType);
      f.Should().NotBeNull();
      f.NFFFileType.Should().Be(fileType);
    }

    [Fact]
    public void Creation3()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile, TNFFFileVersion.nffVersion1_6);
      f.NFFFileType.Should().Be(TNFFFileType.nffSVLFile);
      f.FileVersion.Should().Be(TNFFFileVersion.nffVersion1_6);
    }

    [Fact]
    public void Creation4()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile, TNFFFileVersion.nffVersion1_6, 100);
      f.NFFFileType.Should().Be(TNFFFileType.nffSVLFile);
      f.FileVersion.Should().Be(TNFFFileVersion.nffVersion1_6);
      f.GridSize.Should().Be(100);
    }

    [Fact]
    public void Load_CERA()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "CERA.SVL")).Should().Be(true);
    }

    [Fact]
    public void Load_Dimensions2012_LargeSiteRoad()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "Large Sites Road - Trimble Road.svl")).Should().Be(true);
    }

    [Fact]
    public void Load_Dimensions2012_TopConRoad()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "Topcon Road - Topcon Phil.svl")).Should().Be(true);
    }

    [Fact]
    public void Load_Dimensions2012_Milling()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "Milling - Milling.svl")).Should().Be(true);
    }
  }
}

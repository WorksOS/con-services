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
      var f = new NFFFile();
      f.Should().NotBeNull();
    }

    [Theory]
    [InlineData(NFFFileType.nffSVDFile)]
    [InlineData(NFFFileType.nffSVLFile)]
    public void Creation2(NFFFileType fileType)
    {
      var f = new NFFFile(fileType);
      f.Should().NotBeNull();
      f.NFFFileType.Should().Be(fileType);
    }

    [Fact]
    public void Creation3()
    {
      var f = new NFFFile(NFFFileType.nffSVLFile, NFFFileVersion.nffVersion1_6);
      f.NFFFileType.Should().Be(NFFFileType.nffSVLFile);
      f.FileVersion.Should().Be(NFFFileVersion.nffVersion1_6);
    }

    [Fact]
    public void Creation4()
    {
      var f = new NFFFile(NFFFileType.nffSVLFile, NFFFileVersion.nffVersion1_6, 100);
      f.NFFFileType.Should().Be(NFFFileType.nffSVLFile);
      f.FileVersion.Should().Be(NFFFileVersion.nffVersion1_6);
      f.GridSize.Should().Be(100);
    }

    [Theory]
    [InlineData("CERA.SVL")]
    [InlineData("Large Sites Road - Trimble Road.svl")]
    [InlineData("Topcon Road - Topcon Phil.svl")]
    [InlineData("Milling - Milling.svl")]
    public void Load_Files(string fileName)
    {
      var f = new NFFFile(NFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", fileName)).Should().Be(true);

      f.ErrorStatus.Should().Be(NFFErrorStatus.nffe_OK);
      f.GuidanceAlignments.Should().NotBeNull();
      f.GuidanceAlignments.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Load_CERA()
    {
      var f = new NFFFile(NFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "CERA.SVL")).Should().Be(true);
    }

    [Fact]
    public void Load_Dimensions2012_LargeSiteRoad()
    {
      var f = new NFFFile(NFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "Large Sites Road - Trimble Road.svl")).Should().Be(true);
    }

    [Fact]
    public void Load_Dimensions2012_TopConRoad()
    {
      var f = new NFFFile(NFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "Topcon Road - Topcon Phil.svl")).Should().Be(true);
    }

    [Fact]
    public void Load_Dimensions2012_Milling()
    {
      var f = new NFFFile(NFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "Common", "Milling - Milling.svl")).Should().Be(true);
    }

    [Theory]
    [InlineData("CERA.SVL", NFFFileType.nffSVLFile, NFFFileVersion.nffVersion1_6)]
    [InlineData("Large Sites Road - Trimble Road.svl", NFFFileType.nffSVLFile, NFFFileVersion.nffVersion1_6)]
    [InlineData("Topcon Road - Topcon Phil.svl", NFFFileType.nffSVLFile, NFFFileVersion.nffVersion1_6)]
    [InlineData("Milling - Milling.svl", NFFFileType.nffSVLFile, NFFFileVersion.nffVersion1_6)]
    public void CreateFromFile(string fileName, NFFFileType fileType, NFFFileVersion fileVersion)
    {
      var f = NFFFile.CreateFromFile(Path.Combine("TestData", "Common", fileName));
      f.ErrorStatus.Should().Be(NFFErrorStatus.nffe_OK);

      f.NFFFileType.Should().Be(fileType);
      f.FileVersion.Should().Be(fileVersion);
    }
  }
}

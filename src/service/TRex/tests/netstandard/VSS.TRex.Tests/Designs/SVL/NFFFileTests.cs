using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public void Load()
    {
      var f = new TNFFFile(TNFFFileType.nffSVLFile);
      f.LoadFromFile(Path.Combine("TestData", "CERA.SVL")).Should().Be(true);
    }
  }
}

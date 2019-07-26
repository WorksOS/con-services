using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace VSS.TRex.Designs.SVL.Tests
{
  public class SVLAlignmentBoundaryDeterminatorTests
  {
    [Fact]
    public void Creation_FailWithNullAlignment()
    {
      Action act = () => _ = new SVLAlignmentBoundaryDeterminator(null, 0, 1, 0, 0);
      act.Should().Throw<ArgumentException>().WithMessage($"Alignment cannot be null in constructor for {nameof(SVLAlignmentBoundaryDeterminator)}");
    }

    [Theory]
    [InlineData("CERA.SVL", TNFFFileType.nffSVLFile, TNFFFileVersion.nffVersion1_6)]
    [InlineData("Large Sites Road - Trimble Road.svl", TNFFFileType.nffSVLFile, TNFFFileVersion.nffVersion1_6)]
    [InlineData("Topcon Road - Topcon Phil.svl", TNFFFileType.nffSVLFile, TNFFFileVersion.nffVersion1_6)]
    [InlineData("Milling - Milling.svl", TNFFFileType.nffSVLFile, TNFFFileVersion.nffVersion1_6)]
    public void Creation(string fileName, TNFFFileType fileType, TNFFFileVersion fileVersion)
    {
      var f = TNFFFile.CreateFromFile(Path.Combine("TestData", "Common", fileName));
      f.ErrorStatus.Should().Be(TNFFErrorStatus.nffe_OK);

      f.NFFFileType.Should().Be(fileType);
      f.FileVersion.Should().Be(fileVersion);

      var master = f.GuidanceAlignments?.Where(x => x.IsMasterAlignment()).FirstOrDefault();
      var determinator = new SVLAlignmentBoundaryDeterminator(master, master.StartStation, master.StartStation, -1, 1);

      determinator.Should().NotBeNull();
    }
  }
}

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Designs.SVL.Tests
{
  public class SVLAlignmentBoundaryDeterminatorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation_FailWithNullAlignment()
    {
      Action act = () => _ = new SVLAlignmentBoundaryDeterminator(null, 0, 1, 0, 0);
      act.Should().Throw<ArgumentException>().WithMessage($"Alignment cannot be null in constructor for {nameof(SVLAlignmentBoundaryDeterminator)}");
    }

    [Theory]
    [InlineData("CERA.SVL", NFFFileType.SVLFile, NFFFileVersion.Version1_6)]
    [InlineData("Large Sites Road - Trimble Road.svl", NFFFileType.SVLFile, NFFFileVersion.Version1_6)]
    [InlineData("Topcon Road - Topcon Phil.svl", NFFFileType.SVLFile, NFFFileVersion.Version1_6)]
    [InlineData("Milling - Milling.svl", NFFFileType.SVLFile, NFFFileVersion.Version1_6)]
    public void DetermineBoundary(string fileName, NFFFileType fileType, NFFFileVersion fileVersion)
    {
      var f = NFFFile.CreateFromFile(Path.Combine("TestData", "Common", fileName));
      f.ErrorStatus.Should().Be(NFFErrorStatus.OK);

      f.NFFFileType.Should().Be(fileType);
      f.FileVersion.Should().Be(fileVersion);

      var master = f.GuidanceAlignments?.Where(x => x.IsMasterAlignment()).FirstOrDefault();
      var determinator = new SVLAlignmentBoundaryDeterminator(master, master.StartStation, master.EndStation, -1, 1);

      determinator.Should().NotBeNull();

      determinator.DetermineBoundary(out DesignProfilerRequestResult calcResult, out Fence fence);

      calcResult.Should().Be(DesignProfilerRequestResult.OK);
      fence.Should().NotBeNull();
    }
  }
}

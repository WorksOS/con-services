using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests.SubGrids
{
  public class SubGridStreamHeaderTests
  {
    [Fact]
    public void Creation()
    {
      var header = new SubGridStreamHeader();
      header.Should().NotBeNull();
    }

    [Fact]
    public void IdentifierMatches()
    {
      var header = new SubGridStreamHeader();

      header.Identifier = SubGridStreamHeader.kICServerSubGridLeafFileMoniker;
      header.IdentifierMatches(SubGridStreamHeader.kICServerSubGridLeafFileMoniker).Should().BeTrue();

      header.Identifier = SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker;
      header.IdentifierMatches(SubGridStreamHeader.kICServerSubGridDirectoryFileMoniker).Should().BeTrue();
    }

    [Fact]
    public void Serialization()
    {
      var header = new SubGridStreamHeader
      {
        Identifier = SubGridStreamHeader.kICServerSubGridLeafFileMoniker,
        StartTime = DateTime.UtcNow.AddMinutes(-1),
        EndTime = DateTime.UtcNow,
        Flags = SubGridStreamHeader.kSubGridHeaderFlag_IsSubGridDirectoryFile,
        LastUpdateTimeUTC = DateTime.UtcNow
      };

      using var ms = new MemoryStream();
      using var bw = new BinaryWriter(ms);

      header.Write(bw);

      ms.Position = 0;
      using var br = new BinaryReader(ms);
      var newHeader = new SubGridStreamHeader(br);

      newHeader.Identifier.Should().BeEquivalentTo(header.Identifier);
      newHeader.StartTime.Should().Be(header.StartTime);
      newHeader.EndTime.Should().Be(header.EndTime);
      newHeader.Flags.Should().Be(header.Flags);
      newHeader.LastUpdateTimeUTC.Should().Be(header.LastUpdateTimeUTC);
      newHeader.IsSubGridDirectoryFile.Should().Be(header.IsSubGridDirectoryFile);
      newHeader.IsSubGridSegmentFile.Should().Be(header.IsSubGridSegmentFile);
    }
  }
}

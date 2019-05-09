using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Server
{
  public class SubGridDirectoryTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var dir = new SubGridDirectory();

      dir.ExistsInPersistentStore.Should().BeFalse();
      dir.GlobalLatestCells.Should().BeNull();
      dir.IsMutable.Should().BeFalse();
      dir.SegmentDirectory.Should().NotBeNull();
    }

    [Fact(Skip = "Obsolete")]
    public void Read_FailWithNoGlobalLatestCells()
    {
      var dir = new SubGridDirectory();

      Action act = () => dir.Read(new BinaryReader(new MemoryStream()));
      act.Should().Throw<TRexSubGridIOException>().WithMessage("Cannot read sub grid directory without global latest values available");
    }

    [Fact(Skip = "Obsolete")]
    public void Write_FailWithNoGlobalLatestCells()
    {
      var dir = new SubGridDirectory();

      Action act = () => dir.Write(new BinaryWriter(new MemoryStream()));
      act.Should().Throw<TRexSubGridIOException>().WithMessage("Cannot write sub grid directory without global latest values available");
    }

    [Fact(Skip = "Obsolete")]
    public void Write_FailWithNoSegmentsInDirectory()
    {
      var dir = new SubGridDirectory();
      dir.AllocateGlobalLatestCells();

      Action act = () => dir.Write(new BinaryWriter(new MemoryStream()));
      act.Should().Throw<TRexSubGridIOException>().WithMessage("Writing a segment directory with no segments");
    }
  }
}

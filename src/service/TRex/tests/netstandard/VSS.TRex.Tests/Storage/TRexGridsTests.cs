using FluentAssertions;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;
using Xunit;

namespace VSS.TRex.Tests.Storage
{
  public class TRexGridsTests
  {
    [Fact]
    public void GridNames()
    {
      TRexGrids.GridNames().Should().Contain(TRexGrids.GridName(StorageMutability.Mutable), TRexGrids.GridName(StorageMutability.Immutable));
    }

    [Fact]
    public void MutableGridName()
    {
      TRexGrids.GridName(StorageMutability.Mutable).Should().NotBeNullOrWhiteSpace();
      TRexGrids.MutableGridName().Should().Be(TRexGrids.GridName(StorageMutability.Mutable));
    }

    [Fact]
    public void ImmutableGridName()
    {
      TRexGrids.GridName(StorageMutability.Immutable).Should().NotBeNullOrWhiteSpace();
      TRexGrids.MutableGridName().Should().Be(TRexGrids.GridName(StorageMutability.Mutable));
    }
  }
}

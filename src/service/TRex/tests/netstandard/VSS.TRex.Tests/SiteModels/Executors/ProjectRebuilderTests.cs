using System;
using FluentAssertions;
using VSS.TRex.SiteModels;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderTests
  {
    [Fact]
    public void Creation()
    {
      var rebuilder = new SiteModelRebuilder(Guid.NewGuid());
      rebuilder.Should().NotBeNull();
    }
  }
}

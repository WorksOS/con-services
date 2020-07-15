using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.SiteModelChangeMaps;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeProcessorItemHandlerTests_NoDI
  {
    [Fact]
    public void Creation_NoIgnite()
    {
      DIBuilder.Eject(); // Be doubly sure

      Action act = () =>
      {
        var _ = new SiteModelChangeProcessorItemHandler();
      };

      if (DIContext.DefaultIsRequired)
        act.Should().Throw<Exception>().WithMessage("DIContext service provider not available");
      else
        act.Should().Throw<TRexException>().WithMessage("Failed to obtain immutable Ignite reference");
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests
{
  public class TRexServerConfigTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Instance()
    {
      TRexServerConfig.Instance().Should().NotBeNull();
    }
  }
}

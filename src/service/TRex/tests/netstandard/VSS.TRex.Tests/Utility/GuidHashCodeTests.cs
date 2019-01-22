using System;
using FluentAssertions;
using VSS.TRex.Common.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Utility
{
  public class GuidHashCodeTests
  {
    [Fact]
    public void Test_GuidHashCode_Hash_Empty()
    {
      // An empty guid (all zeros) should always be 0
      GuidHashCode.Hash(Guid.Empty).Should().Be(0);
    }

    [Fact]
    public void Test_GuidHashCode_HashCode_NotEmpty()
    {
      Guid g = Guid.NewGuid();

      GuidHashCode.Hash(g).Should().NotBe(0, $"for Guid value {g}");
    }

    [Theory]
    [InlineData("{8E2561FB-18FC-412F-8F79-D74EE6B4E314}", 1857306261)]
    [InlineData("{3C4B1183-F028-4A86-AFF1-EA528815D9E4}", -1945764160)]
    [InlineData("{0804F7E0-B7B5-44DB-9F76-1185E90768DB}", 590456338)]
    public void Test_GuidHashCode_HashCode_Divergence(string guidString, int expectedHash)
    {
      Guid g = new Guid(guidString);
      GuidHashCode.Hash(g).Should().Be(expectedHash, $"for Guid value {g}");
    }
  }
}

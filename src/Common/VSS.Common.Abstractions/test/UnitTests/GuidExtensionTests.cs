using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.Common.Abstractions.Extensions;
using Xunit;

namespace VSS.Common.Abstractions.UnitTests
{
  public class GuidExtensionTests
  {
    [Theory]
    [InlineData("9F214DD0-05F1-4F52-B1ED-EB18FCDBAD64", 7254696450560093617L)]
    [InlineData("A82110BD-ABED-4E5B-88CD-28083212ACFA", -383911862138778232L)]
    [InlineData("F61A53DB-EE92-4B33-BB61-E03ED59A5D6F", 8024740351717695931L)]
    [InlineData("0744A365-4952-4C0C-AEEF-99124B01CACC", -3690135522730446930L)]
    [InlineData("A1623C4A-81F1-4068-A7CA-340CC960AFE6", -1824132907452282201L)]
    [InlineData("2D7FB893-CE32-4911-A81E-226006A30B77", 8578129163035221672L)]
    public void ShouldConvertToTheSameLong(Guid g, long l)
    {
      // We are using these longs as legacy raptor IDs for backwards compability
      // They MUST return the same value every time.
      var result = g.ToLegacyId();

      result.Should().Be(l);
    }

    [Fact]
    public void TestCollision()
    {
      // We should have no more than a dozen projects (Guids) per customer
      // But we need to ensure we can handle a lot more.
      // In the situation we actually get a collision, we will need to update a GUID
      // But hopefully by then, we don't need to support legacy IDs
      var hash = new HashSet<long>();
      for (var i = 0; i < 100000; i++)
      {
        var g = Guid.NewGuid();
        var l = g.ToLegacyId();
        if (hash.Contains(l))
          false.Should().BeTrue($"Index: {i} Guid {g} has a Long Collision of {l}");
        hash.Add(l);
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Tests.Utility
{
  public class MachineSerialUtilitiesTests
  {

    [Theory]
    [InlineData("123abcSM", "CB430")]
    [InlineData("123abcSV", "CB450")]
    [InlineData("123abcSW", "CB460")]
    [InlineData("123abcYU", "EC520")]
    public void Test_CB430(string serial, string expectedModel)
    {
      MachineSerialUtilities.MapSerialToModel(serial).Should().Be(expectedModel);
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("")]
    public void Test_InvalidMappings(string serial)
    {
      Action act = () => MachineSerialUtilities.MapSerialToModel(serial);

      act.Should().Throw<ArgumentException>().WithMessage("No mapping exists for this serial number");
    }


  }
}

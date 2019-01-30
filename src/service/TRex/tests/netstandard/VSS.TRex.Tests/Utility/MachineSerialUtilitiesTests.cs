using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Common.Types;

namespace VSS.TRex.Tests.Utility
{
  public class MachineSerialUtilitiesTests
  {

    [Theory]
    [InlineData("123abcSM", MachineControlPlatformType.CB430)]
    [InlineData("123abcSV", MachineControlPlatformType.CB450)]
    [InlineData("123abcSW", MachineControlPlatformType.CB460)]
    [InlineData("123abcYU", MachineControlPlatformType.EC520)]
    public void Test_SerialToPlatform(string serial, MachineControlPlatformType expectedModel)
    {
      MachineSerialUtilities.MapSerialToModel(serial).Should().Be(expectedModel);
    }


    [Theory]
    [InlineData(MachineControlPlatformType.CB430, "CB430")]
    [InlineData(MachineControlPlatformType.CB450, "CB450")]
    [InlineData(MachineControlPlatformType.CB460, "CB460")]
    [InlineData(MachineControlPlatformType.EC520, "EC520")]
    public void Test_StringRepresentation_of_Platform(MachineControlPlatformType type, string expectedModel)
    {
      type.ToString().Should().Be(expectedModel);
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

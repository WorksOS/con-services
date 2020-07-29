using System;
using Xunit;
using FluentAssertions;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Enums;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Tests.Utility
{
  public class MachineSerialUtilitiesTests
  {

    [Theory]
    [InlineData("123abcSM", CWSDeviceTypeEnum.CB430)]
    [InlineData("123abcSV", CWSDeviceTypeEnum.CB450)]
    [InlineData("123abcSW", CWSDeviceTypeEnum.CB460)]
    [InlineData("123J001YU", CWSDeviceTypeEnum.EC520W)]
    [InlineData("123J501YU", CWSDeviceTypeEnum.EC520)]
    [InlineData("123abcYU", CWSDeviceTypeEnum.Unknown)]
    [InlineData("ZZ", CWSDeviceTypeEnum.Unknown)]
    public void Test_SerialToPlatform(string serial, CWSDeviceTypeEnum expectedModel)
    {
      MachineSerialUtilities.MapSerialToModel(serial).Should().Be(expectedModel);
    }


    [Theory]
    [InlineData(CWSDeviceTypeEnum.CB430, "CB430")]
    [InlineData(CWSDeviceTypeEnum.CB450, "CB450")]
    [InlineData(CWSDeviceTypeEnum.CB460, "CB460")]
    [InlineData(CWSDeviceTypeEnum.EC520, "EC520")]
    [InlineData(CWSDeviceTypeEnum.EC520W, "EC520-W")]
    [InlineData(CWSDeviceTypeEnum.Unknown, "Unknown")]
    public void Test_StringRepresentation_of_Platform(CWSDeviceTypeEnum type, string expectedModel)
    {
      type.GetEnumMemberValue().Should().Be(expectedModel);
    }


    [Theory]
    [InlineData("")]
    public void Test_InvalidMappings(string serial)
    {
      Action act = () => MachineSerialUtilities.MapSerialToModel(serial);

      act.Should().Throw<ArgumentException>().WithMessage("No mapping exists for this serial number");
    }


  }
}

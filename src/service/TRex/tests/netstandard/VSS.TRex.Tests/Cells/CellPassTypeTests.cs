using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Cells
{
  public class CellPassTypeTests
  {
    [Fact]
    public void Test_CellPassTypeSet_Creation()
    {
      PassTypeSet PassTypes = PassTypeSet.None;
      Assert.True(PassTypes == PassTypeSet.None);

      PassTypes |= PassTypeSet.Front;
      Assert.True(PassTypes == PassTypeSet.Front);

      PassTypes |= PassTypeSet.Rear;
      Assert.True(PassTypes == (PassTypeSet.Front | PassTypeSet.Rear));

      PassTypes |= PassTypeSet.Track;
      Assert.True(PassTypes == (PassTypeSet.Front | PassTypeSet.Rear | PassTypeSet.Track));

      PassTypes |= PassTypeSet.Wheel;
      Assert.True(PassTypes == (PassTypeSet.Front | PassTypeSet.Rear | PassTypeSet.Track | PassTypeSet.Wheel));
    }

    [Fact]
    public void Test_CellPassTypeSet_Comparison()
    {
      PassTypeSet PassTypes = PassTypeSet.None;

      PassTypes = PassTypeSet.Front;
      Assert.True(PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Front));

      PassTypes = PassTypeSet.Rear;
      Assert.True(PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Rear));

      PassTypes = PassTypeSet.Track;
      Assert.True(PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Track));

      PassTypes = PassTypeSet.Wheel;
      Assert.True(PassTypeHelper.PassTypeSetContains(PassTypes, PassType.Wheel));
    }

    [Fact]
    public void GetPassType()
    {
      PassTypeHelper.GetPassType(0).Should().Be(PassType.Front);

      PassTypeHelper.GetPassType(1 << (int) GPSFlagBits.GPSSBit6).Should().Be(PassType.Rear);
      PassTypeHelper.GetPassType(1 << (int) GPSFlagBits.GPSSBit7).Should().Be(PassType.Track);
      PassTypeHelper.GetPassType(1 << (int) GPSFlagBits.GPSSBit7 | 1 << (int) GPSFlagBits.GPSSBit6).Should().Be(PassType.Wheel);
    }

    [Theory]
    [InlineData(PassType.Front)]
    [InlineData(PassType.Rear)]
    [InlineData(PassType.Track)]
    [InlineData(PassType.Wheel)]
    public void SetPassType(PassType passType)
    {
      byte value = 0;
      PassTypeHelper.GetPassType(PassTypeHelper.SetPassType(value, passType)).Should().Be(passType);
    }


    [Fact]
    public void SetPassType_Fail()
    {
      byte value = 0;

      Action act = () => PassTypeHelper.SetPassType(value, (PassType) 100);
      act.Should().Throw<ArgumentException>().WithMessage("*Unknown pass type supplied to SetPassType*");
    }
  }
}

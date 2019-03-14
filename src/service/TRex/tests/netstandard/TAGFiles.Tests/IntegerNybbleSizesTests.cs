using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.TAGFiles.Types;
using Xunit;

namespace TAGFiles.Tests
{
  public class IntegerNybbleSizesTests
  {
    [Fact]
    public void IntegerSizes()
    {
      IntegerNybbleSizes.Nybbles(TAGDataType.t4bitInt).Should().Be(1);
      IntegerNybbleSizes.Nybbles(TAGDataType.t4bitUInt).Should().Be(1);
      IntegerNybbleSizes.Nybbles(TAGDataType.t8bitInt).Should().Be(2);
      IntegerNybbleSizes.Nybbles(TAGDataType.t8bitUInt).Should().Be(2);
      IntegerNybbleSizes.Nybbles(TAGDataType.t12bitInt).Should().Be(3);
      IntegerNybbleSizes.Nybbles(TAGDataType.t12bitUInt).Should().Be(3);
      IntegerNybbleSizes.Nybbles(TAGDataType.t16bitInt).Should().Be(4);
      IntegerNybbleSizes.Nybbles(TAGDataType.t16bitUInt).Should().Be(4);
      IntegerNybbleSizes.Nybbles(TAGDataType.t32bitInt).Should().Be(8);
      IntegerNybbleSizes.Nybbles(TAGDataType.t32bitUInt).Should().Be(8);
    }

    [Fact]
    public void IntegerSizes_Invalid()
    {
      foreach (TAGDataType t in Enum.GetValues(typeof(TAGDataType)))
      {
        if (!(t == TAGDataType.t4bitInt ||
        t == TAGDataType.t4bitUInt ||
        t == TAGDataType.t8bitInt ||
        t == TAGDataType.t8bitUInt ||
        t == TAGDataType.t12bitInt ||
        t == TAGDataType.t12bitUInt ||
        t == TAGDataType.t16bitInt ||
        t == TAGDataType.t16bitUInt ||
        t == TAGDataType.t32bitInt ||
        t == TAGDataType.t32bitUInt))
        {
          Action act = () => IntegerNybbleSizes.Nybbles(t);
          act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Unknown integer TAG field type*");
        }
      }
    }
  }
}

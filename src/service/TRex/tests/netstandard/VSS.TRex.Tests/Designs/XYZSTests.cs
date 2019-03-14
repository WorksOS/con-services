using FluentAssertions;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class XYZSTests
  {
    [Fact]
    public void Creation()
    {
      var xyzs = new XYZS();
      xyzs.Station.Should().Be(0);
      xyzs.TriIndex.Should().Be(0);
      xyzs.X.Should().Be(0);
      xyzs.Y.Should().Be(0);
      xyzs.Z.Should().Be(0);
    }

    [Fact]
    public void Creation2()
    {
      var xyzs = new XYZS(1, 2, 3, 4, 5);

      xyzs.X.Should().Be(1);
      xyzs.Y.Should().Be(2);
      xyzs.Z.Should().Be(3);
      xyzs.Station.Should().Be(4);
      xyzs.TriIndex.Should().Be(5);
    }

    [Fact]
    public void Test_ToString()
    {
      var xyzs = new XYZS(1, 2, 3, 4, 5);
      xyzs.ToString().Should().Be($"X:{1:F3}, Y:{2:F3}, Z:{3:F3} Station:{4:F3}, TriIndex:{5}");
    }

    [Fact]
    public void Test_Equals()
    {
      var xyzs = new XYZS(1, 2, 3, 4, 5);
      var xyzs2 = new XYZS(1, 2, 3, 4, 5);
      var xyzs3 = new XYZS(2, 3, 4, 5, 6);

      xyzs.Equals(xyzs2).Should().BeTrue();
      xyzs.Equals(xyzs3).Should().BeFalse();
    }
  }
}

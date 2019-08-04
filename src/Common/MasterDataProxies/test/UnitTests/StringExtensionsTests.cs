using Xunit;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class StringExtensionsTests
  {
    [Fact]
    public void CanCallTruncateWithNullString()
    {
      string str = null;
      var truncStr = str.Truncate(2);
      Assert.Null(truncStr);
    }

    [Fact]
    public void CanCallTruncateWithZeroMaxLength()
    {
      string str = "some string";
      var truncStr = str.Truncate(0);
      Assert.Equal(str, truncStr);
    }

    [Fact]
    public void CanTruncateStringWithEllipsis()
    {
      string str = "some string";
      var truncStr = str.Truncate(8);
      Assert.Equal("some str...", truncStr);
    }

    [Fact]
    public void NoTruncationForShortString()
    {
      string str = "some string";
      var truncStr = str.Truncate(20);
      Assert.Equal(str, truncStr);
    }
  }
}

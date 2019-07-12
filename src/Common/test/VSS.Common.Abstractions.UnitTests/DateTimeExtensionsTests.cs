using System;
using VSS.Common.Abstractions.Extensions;
using Xunit;

namespace VSS.Common.Abstractions.UnitTests
{
  public class DateTimeExtensionsTests
  {
    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Utc)]
    public void UtcDateShouldConvertToIso8601(DateTimeKind kind)
    {
      var date = new DateTime(2019, 03, 27, 11, 28, 33, kind);
      var expected = "2019-03-27T11:28:33Z";
      var actual = date.ToIso8601DateTimeString();
      Assert.Equal(expected, actual);
    }

    [Fact]
    public void LocalDateShouldNotConvertToIso8601()
    {
      var date = new DateTime(2019, 03, 27, 11, 28, 33, DateTimeKind.Local);
      Assert.Throws<ArgumentException>(() => date.ToIso8601DateTimeString());
    }
  }
}

using System;
using VSS.Common.Abstractions.Extensions;
using FluentAssertions;
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
      date.ToIso8601DateTimeString().Should().Be("2019-03-27T11:28:33Z");
    }

    [Theory]
    [InlineData("2019-07-30T23:35:12Z", DateTimeKind.Local, "2019-07-30T23:35:12Z")]
    [InlineData("2019-01-01T00:00:00", DateTimeKind.Unspecified, "2019-01-01T00:00:00Z")]
    public void ParsedUtcDateStringShouldConvertToIso8601(string dateToParse, DateTimeKind expectedParseKind, string expectedUtcString)
    {
      var localDate = DateTime.Parse(dateToParse);
      localDate.Kind.Should().Be(expectedParseKind);
      localDate.ToIso8601DateTimeString().Should().Be(expectedUtcString);
    }



  }
}

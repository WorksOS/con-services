using System;
using VSS.Common.Abstractions.Extensions;
using Xunit;

namespace VSS.Common.Abstractions.UnitTests
{
  public class StringExtensionsTests
  {
    [Theory]
    [InlineData("dummy.ttm", "dummy_2019-03-27T112833Z.ttm")]
    [InlineData("some_path/dummy.tif", "dummy_2019-03-27T112833Z.tif")]
    public void ShouldIncludeSurveyedUtcInName(string fileName, string expectedFileName)
    {
      var dateTime = new DateTime(2019, 03, 27, 11, 28, 33, DateTimeKind.Utc);
      var actual = fileName.IncludeSurveyedUtcInName(dateTime);
      Assert.Equal(expectedFileName, actual);
    }
  }
}

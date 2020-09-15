using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using FluentAssertions;
using Xunit;

namespace CCSS.WorksOS.Reports.UnitTests
{
  public class Claytons
  {
    [Fact]
    public void ClaytonsTest() => new ReportRequest().Should().NotBeNull();
  }
}

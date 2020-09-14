using CCSS.WorksOS.Healthz.Abstractions.Models.Request;
using FluentAssertions;
using Xunit;

namespace CCSS.WorksOS.Healthz.UnitTests
{
  public class Claytons
  {
    [Fact]
    public void ClaytonsTest() => new HealthStatusRequestDto().Should().NotBeNull();
  }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using IntegrationTests;
//using Xunit;

//namespace CCSS.WorksOS.Healthz.IntegrationTests.WebAPITests
//{
//  public class HealthCheckControllerTests : IClassFixture<TestFixture>
//  {
//    private TestFixture _fixture;

//    public HealthCheckControllerTests(TestFixture testFixture)
//    {
//      _fixture = testFixture;
//    }

//    [Fact]
//    public async Task Check_aggregated_service_status()
//    {
//      var httpResponse = await _fixture.RestClient.SendAsync(
//     "/api/v1/service/status",
//     HttpMethod.Get);

//      httpResponse.IsSuccessStatusCode.Should().BeTrue();
//    }
//  }
//}

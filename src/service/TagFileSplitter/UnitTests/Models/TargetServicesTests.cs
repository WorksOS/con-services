using System.Linq;
using System.Net;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using FluentAssertions;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace CCSS.TagFileSplitter.UnitTests.Models
{
  public class TargetServicesTests
  {
    [Theory]
    [InlineData("", 0)]
    [InlineData("Productivity3D,tagfiles,tagfiles/direct", 1)]
    [InlineData("Productivity3D,tagfiles,tagfiles/direct;Productivity3DVSS,tagfiles2,tagfiles2/direct", 2)]
    public void TargetServices_SetServices(string configString, int expectedTargetCount)
    {
      var targetServices = new TargetServices();
      var count = targetServices.SetServices(configString);

      count.Should().Be(expectedTargetCount);
      if (count > 0)
      {
        var target1 = targetServices.Services.FindAll(x => x.ApiService == ApiService.Productivity3D);
        target1.Count().Should().Be(1);
        target1[0].AutoRoute.Should().Be("tagfiles");
        target1[0].DirectRoute.Should().Be("tagfiles/direct");
        if (count > 1)
        {
          var target2 = targetServices.Services.FindAll(x => x.ApiService == ApiService.Productivity3DVSS);
          target2.Count().Should().Be(1);
          target2[0].AutoRoute.Should().Be("tagfiles2");
          target2[0].DirectRoute.Should().Be("tagfiles2/direct");
        }
      }
    }

    [Theory]
    [InlineData("Productivity3D,tagfiles,tagfiles/direct", "Productivity3DVSS,tagfiles2,tagfiles2/direct", 1, 2)]
    [InlineData("Productivity3D,tagfiles,tagfiles/direct;Productivity3DVSS,tagfiles2,tagfiles2/direct", "None,tagfiles3,tagfiles3/direct", 2, 3)]
    public void TargetServices_AppendServices(string firstConfigString, string secondConfigString, int expectedFirstTargetCount, int expectedSecondTargetCount)
    {
      var targetServices = new TargetServices();
      var count = targetServices.AppendServices(firstConfigString);
      count.Should().Be(expectedFirstTargetCount);
      count = targetServices.AppendServices(secondConfigString);
      count.Should().Be(expectedSecondTargetCount);

      if (count > 1)
      {
        var target1 = targetServices.Services.FindAll(x => x.ApiService == ApiService.Productivity3D);
        target1.Count().Should().Be(1);
        target1[0].AutoRoute.Should().Be("tagfiles");
        target1[0].DirectRoute.Should().Be("tagfiles/direct");
        var target2 = targetServices.Services.FindAll(x => x.ApiService == ApiService.Productivity3DVSS);
        target2.Count().Should().Be(1);
        target2[0].AutoRoute.Should().Be("tagfiles2");
        target2[0].DirectRoute.Should().Be("tagfiles2/direct");

        if (count > 2)
        {
          var target3 = targetServices.Services.FindAll(x => x.ApiService == ApiService.None);
          target3.Count().Should().Be(1);
          target3[0].AutoRoute.Should().Be("tagfiles3");
          target3[0].DirectRoute.Should().Be("tagfiles3/direct");
        }
      }
    }

    [Theory]
    [InlineData("", 0, "No target services are configured")]
    [InlineData("None,tagfiles2,tagfiles2/direct", 1, "Target service: None is not a supported type")]
    [InlineData("Productivity3D,tagfiles,tagfiles/direct;AssetMgmt3D,tagfiles2, tagfiles2/direct", 2, "Target service: AssetMgmt3D is not a supported type")]
    [InlineData("Productivity3D,,tagfiles/direct;AssetMgmt3D,tagfiles2,tagfiles2/direct", 2, "Auto route missing")]
    [InlineData("Productivity3D,tagfiles,;AssetMgmt3D,tagfiles2,tagfiles2/direct", 2, "Direct route missing")]
    public void TargetServices_Validate(string configString, int targetCount, string errorMessage)
    {
      var targetServices = new TargetServices();
      var count = targetServices.AppendServices(configString);
      count.Should().Be(targetCount);
      var ex = Assert.Throws<ServiceException>(() => targetServices.Validate());

      ex.Should().NotBeNull();
      ex.Code.Should().Be(HttpStatusCode.InternalServerError);
      ex.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      ex.GetResult.Message.Should().Be(errorMessage);
    }
  }
}

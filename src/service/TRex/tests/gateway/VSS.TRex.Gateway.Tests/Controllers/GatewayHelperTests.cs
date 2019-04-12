using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;
using Moq;

namespace VSS.TRex.Gateway.Tests.Controllers
{
  public class GatewayHelperTests : DITagFileFixture, IDisposable
  {
    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7")]
    public void GatewayHelper_NoSiteModel_Fail(Guid projectUid)
    {
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns((ISiteModel)null);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Complete();

      var ex = Assert.Throws<ServiceException>(() => GatewayHelper.ValidateAndGetSiteModel("ValidateReportData_GriddedNoSiteModel_Fail", projectUid));
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      ex.GetResult.Message.Should().Be($"ValidateReportData_GriddedNoSiteModel_Fail: SiteModel: {projectUid} not found ");
    }

    [Theory]
    [InlineData("17e6bd66-54d8-4651-8907-88b15d81b2d7")]
    public void GatewayHelper_InvalidSiteModelID_Fail(Guid projectUid)
    {
      var mockSiteModels = new Mock<ISiteModels>();
      mockSiteModels.Setup(x => x.GetSiteModel(It.IsAny<Guid>(), It.IsAny<bool>())).Returns((ISiteModel)null);

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ISiteModels>(mockSiteModels.Object))
        .Complete();

      var siteModelID = Guid.Empty;
      var ex = Assert.Throws<ServiceException>(() => GatewayHelper.ValidateAndGetSiteModel("ValidateReportData_GriddedNoSiteModel_Fail", siteModelID));
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      ex.GetResult.Message.Should().Be($"siteModel ID: {siteModelID} format is invalid.");
    }

    [Fact]
    public void GatewayHelper_NoMachines()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
     
      var contributingMachines = new List<Guid?>();
      GatewayHelper.ValidateMachines(contributingMachines, siteModel);
    }

    [Fact]
    public void GatewayHelper_NoContributingMachines()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      
      var contributingMachines = new List<Guid?>();
      GatewayHelper.ValidateMachines(contributingMachines, siteModel);
    }

    [Fact]
    public void GatewayHelper_ContributingMachineEmptyGuid()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var contributingMachines = new List<Guid?> { new Guid()};
      var result = Assert.Throws<ServiceException>(() => GatewayHelper.ValidateMachines(contributingMachines, siteModel));
      result.Code.Should().Be(HttpStatusCode.BadRequest);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      result.GetResult.Message.Should().Be($"ValidateMachines: SiteModel: {siteModel.ID} machineUid not found: {contributingMachines[0]}");
    }

    [Fact]
    public void GatewayHelper_ContributingMachineMissing()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var contributingMachines = new List<Guid?>
      {
        new Guid(Guid.NewGuid().ToString())
      };
      var result = Assert.Throws<ServiceException>(() => GatewayHelper.ValidateMachines(contributingMachines, siteModel));
      result.Code.Should().Be(HttpStatusCode.BadRequest);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      result.GetResult.Message.Should().Be($"ValidateMachines: SiteModel: {siteModel.ID} machineUid not found: {contributingMachines[0]}");
    }

    [Fact]
    public void GatewayHelper_ContributingMachineExists()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var contributingMachines = new List<Guid?> { new Guid(machine.ID.ToString()) };
      GatewayHelper.ValidateMachines(contributingMachines, siteModel);
    }

    [Fact]
    public void GatewayHelper_ContributingMachineMultiExists()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, true, Guid.NewGuid());
      var machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var contributingMachines = new List<Guid?>
      {
        new Guid(machine3.ID.ToString()),
        new Guid(machine2.ID.ToString())
      };
      GatewayHelper.ValidateMachines(contributingMachines, siteModel);
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}



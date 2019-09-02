using System;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Events.Models;
using VSS.TRex.Gateway.WebApi.Controllers;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Gateway.Tests.EditData
{
  public class GetDataEditTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void GetOverrideEvents_MissingSiteModel()
    {
      var projectUid = Guid.NewGuid();
      var controller = CreateController();
      var result = Assert.Throws<ServiceException>(() => controller.GetDataEdit(projectUid, null));

      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, result.GetResult.Code);
      Assert.Equal($"Project {projectUid} does not exist", result.GetResult.Message);
    }

    [Fact]
    public void GetOverrideEvents_MissingAsset()
    {
      var siteModel = CreateSiteModelWithMachines();
      var assetUid = Guid.NewGuid();

      var controller = CreateController();
      var result = Assert.Throws<ServiceException>(() => controller.GetDataEdit(siteModel.ID, assetUid));

      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, result.GetResult.Code);
      Assert.Equal($"Asset {assetUid} does not exist in project {siteModel.ID}", result.GetResult.Message);
    }

    [Fact]
    public void GetOverrideEvents_NoEventsForProject()
    {
      var siteModel = CreateSiteModelWithMachines();

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, null) as TRexEditDataResult;
      CheckSuccessfulResult(result, 0);
    }

    [Fact]
    public void GetOverrideEvents_NoEventsForAsset()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, bulldozer.ID) as TRexEditDataResult;
      CheckSuccessfulResult(result, 0);
    }

    [Fact]
    public void GetOverrideEvents_WithDesignEventsForProject()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      var excavator = siteModel.Machines.Locate("Excavator", false);
      var refDate = DateTime.UtcNow;
      AddDesignEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, null) as TRexEditDataResult;
      CheckSuccessfulResult(result, 3);

      CheckValidDataEdit(result.DataEdits[0], bulldozer.ID, refDate.AddMinutes(-90), refDate.AddMinutes(-60), "design 3", null);
      CheckValidDataEdit(result.DataEdits[1], bulldozer.ID, refDate.AddMinutes(-40), refDate.AddMinutes(-20), "design 1", null);
      CheckValidDataEdit(result.DataEdits[2], excavator.ID, refDate.AddMinutes(-80), refDate.AddMinutes(-50), "design 1", null);
    }

    [Fact]
    public void GetOverrideEvents_WithLayerEventsForProject()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      var excavator = siteModel.Machines.Locate("Excavator", false);
      var refDate = DateTime.UtcNow;
      AddLayerEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, null) as TRexEditDataResult;
      CheckSuccessfulResult(result, 3);

      CheckValidDataEdit(result.DataEdits[0], bulldozer.ID, refDate.AddMinutes(-90), refDate.AddMinutes(-60), null, 2);
      CheckValidDataEdit(result.DataEdits[1], bulldozer.ID, refDate.AddMinutes(-60), refDate.AddMinutes(-50), null, 4);
      CheckValidDataEdit(result.DataEdits[2], excavator.ID, refDate.AddMinutes(-80), refDate.AddMinutes(-50), null, 3);
    }

    [Fact]
    public void GetOverrideEvents_WithDesignAndLayerEventsForProject()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      var excavator = siteModel.Machines.Locate("Excavator", false);
      var refDate = DateTime.UtcNow;
      AddDesignEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);
      AddLayerEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, null) as TRexEditDataResult;
      CheckSuccessfulResult(result, 4);

      CheckValidDataEdit(result.DataEdits[0], bulldozer.ID, refDate.AddMinutes(-90), refDate.AddMinutes(-60), "design 3", 2);
      CheckValidDataEdit(result.DataEdits[1], bulldozer.ID, refDate.AddMinutes(-40), refDate.AddMinutes(-20), "design 1", null);
      CheckValidDataEdit(result.DataEdits[2], bulldozer.ID, refDate.AddMinutes(-60), refDate.AddMinutes(-50), null, 4);
      CheckValidDataEdit(result.DataEdits[3], excavator.ID, refDate.AddMinutes(-80), refDate.AddMinutes(-50), "design 1", 3);
    }

    [Fact]
    public void GetOverrideEvents_WithDesignEventsForAsset()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      var refDate = DateTime.UtcNow;
      AddDesignEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, bulldozer.ID) as TRexEditDataResult;
      CheckSuccessfulResult(result, 2);

      CheckValidDataEdit(result.DataEdits[0], bulldozer.ID, refDate.AddMinutes(-90), refDate.AddMinutes(-60), "design 3", null);
      CheckValidDataEdit(result.DataEdits[1], bulldozer.ID, refDate.AddMinutes(-40), refDate.AddMinutes(-20), "design 1", null);
    }

    [Fact]
    public void GetOverrideEvents_WithLayerEventsForAsset()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      var refDate = DateTime.UtcNow;
      AddLayerEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, bulldozer.ID) as TRexEditDataResult;
      CheckSuccessfulResult(result, 2);

      CheckValidDataEdit(result.DataEdits[0], bulldozer.ID, refDate.AddMinutes(-90), refDate.AddMinutes(-60), null, 2);
      CheckValidDataEdit(result.DataEdits[1], bulldozer.ID, refDate.AddMinutes(-60), refDate.AddMinutes(-50), null, 4);
    }

    [Fact]
    public void GetOverrideEvents_WithDesignAndLayerEventsForAsset()
    {
      var siteModel = CreateSiteModelWithMachines();
      var bulldozer = siteModel.Machines.Locate("Bulldozer", false);
      var refDate = DateTime.UtcNow;
      AddDesignEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);
      AddLayerEvents(siteModel, refDate, bulldozer.InternalSiteModelMachineIndex);

      var controller = CreateController();
      var result = controller.GetDataEdit(siteModel.ID, bulldozer.ID) as TRexEditDataResult;
      CheckSuccessfulResult(result, 3);

      CheckValidDataEdit(result.DataEdits[0], bulldozer.ID, refDate.AddMinutes(-90), refDate.AddMinutes(-60), "design 3", 2);
      CheckValidDataEdit(result.DataEdits[1], bulldozer.ID, refDate.AddMinutes(-40), refDate.AddMinutes(-20), "design 1", null);
      CheckValidDataEdit(result.DataEdits[2], bulldozer.ID, refDate.AddMinutes(-60), refDate.AddMinutes(-50), null, 4);
    }

    #region privates
    private void CheckSuccessfulResult(TRexEditDataResult result, int count)
    {
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.NotNull(result.DataEdits);
      Assert.Equal(count, result.DataEdits.Count);
    }

    private void CheckValidDataEdit(TRexEditData actual, Guid assetUid, DateTime startDate, DateTime endDate, string designName, int? liftNumber)
    {
      Assert.Equal(assetUid, actual.AssetUid);
      Assert.Equal(startDate, actual.StartUtc);
      Assert.Equal(endDate, actual.EndUtc);
      Assert.Equal(designName, actual.MachineDesignName);
      Assert.Equal(liftNumber, actual.LiftNumber);
    }

    private EditDataController CreateController()
    {
      var mockExceptionHandler = new Mock<IServiceExceptionHandler>();
      var mockConfig = new Mock<IConfigurationStore>();
      return new EditDataController(new NullLoggerFactory(), mockExceptionHandler.Object, mockConfig.Object);
    }

    private ISiteModel CreateSiteModelWithMachines()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();//creates machine "Bulldozer"
      siteModel.Machines.CreateNew("Excavator", "", MachineType.Excavator, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddDesignEvents(ISiteModel siteModel, DateTime refDate, short bulldozerMachineIndex)
    {
      siteModel.SiteModelMachineDesigns.CreateNew("design 1");
      siteModel.SiteModelMachineDesigns.CreateNew("design 2");
      siteModel.SiteModelMachineDesigns.CreateNew("design 3");

      siteModel.MachinesTargetValues[bulldozerMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<int>(refDate.AddMinutes(-60), 3));
      siteModel.MachinesTargetValues[bulldozerMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-40), new OverrideEvent<int>(refDate.AddMinutes(-20), 1));

      var excavator = siteModel.Machines.Locate("Excavator", false);
      siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].DesignOverrideEvents.PutValueAtDate(refDate.AddMinutes(-80), new OverrideEvent<int>(refDate.AddMinutes(-50), 1));
    }

    private void AddLayerEvents(ISiteModel siteModel, DateTime refDate, short bulldozerMachineIndex)
    {
      siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-90), new OverrideEvent<ushort>(refDate.AddMinutes(-60), 2));
      siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-60), new OverrideEvent<ushort>(refDate.AddMinutes(-50), 4));

      var excavator = siteModel.Machines.Locate("Excavator", false);
      siteModel.MachinesTargetValues[excavator.InternalSiteModelMachineIndex].LayerOverrideEvents.PutValueAtDate(refDate.AddMinutes(-80), new OverrideEvent<ushort>(refDate.AddMinutes(-50), 3));
    }
#endregion
  }
}

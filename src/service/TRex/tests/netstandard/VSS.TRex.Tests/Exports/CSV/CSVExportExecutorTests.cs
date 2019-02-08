using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportExecutorTests : IClassFixture<DITagFileFixture>
  {
    // todoJeannie

    [Fact]
    public void CSVExportExecutor_SiteModelNotFound()
    {
      var projectUid = Guid.NewGuid(); // use NewSiteModelGuid to get mocked siteModel
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] {"first machineName"};

      var request = CompactionVetaExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, machineNames);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public void CSVExportExecutor_StartEndDate()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      Assert.Equal(startTime, startEndDate.Item1);
      Assert.Equal(endTime, startEndDate.Item2);
    }

    [Fact]
    public void CSVExportExecutor_EndDateOnly()
    {
      var filter = Filter.CreateFilter(
        new DateTime(2019, 1, 10),
        null,"","",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);

      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, filterResult);
      Assert.Equal(filter.StartUtc, startEndDate.Item1);
      Assert.Equal(endTime, startEndDate.Item2);
    }

    [Fact]
    public void CSVExportExecutor_MultiMachines()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      Assert.Equal(startTime, startEndDate.Item1);
      Assert.Equal(endTime, startEndDate.Item2);
    }

    [Fact]
    public void CSVExportExecutor_MachineNamesNotFound()
    {
      string[] machineNames = new[] {"Test Machine 1"};
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());

      var machinesListGuids = CSVExportHelper.GetRequestedMachines(siteModel, machineNames);
      Assert.Single(machinesListGuids);
      Assert.Equal(machine1.ID, machinesListGuids[0]);
    }
  }
}



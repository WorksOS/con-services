using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.CSVExport
{
  public class CSVExportExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public async Task CSVExportExecutor_SiteModelNotFound()
    {
      var projectUid = Guid.NewGuid(); 
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] {"first machineName"};
      var userPreferences = new UserPreferences();

      var request = new CompactionVetaExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreferences, machineNames, null, null);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.BadRequest);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be($"Site model {projectUid} is unavailable");
    }

    [Fact]
    public async Task CSVExportExecutor_NoCSIB()
    {
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] { "first machineName" };
      var userPreferences = new UserPreferences();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionVetaExportRequest(
        siteModel.ID, null, fileName,
        coordType, outputType, userPreferences, machineNames, null, null);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be("Failed to complete TRex request: CSVExportExecutor with error: Unable to load CSIB for LatLong conversion");
    }

    [Fact]
    public async Task CSVExportExecutor_GotSiteAndFilter()
    {
      var fileName = "gotAFilename";
      var coordType = CoordType.Northeast;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] { "first machineName" };
      var userPreferences = new UserPreferences();
      var filter = new Productivity3D.Filter.Abstractions.Models.Filter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc), "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionVetaExportRequest(
        siteModel.ID, filterResult, fileName,
        coordType, outputType, userPreferences, machineNames, null, null);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      // Mock the CSV export request functionality to return a null CSV export reponse to stimulate tyhe desired internal processing error
      var mockCompute = IgniteMock.Immutable.mockCompute;
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<CSVExportRequestComputeFunc>(), It.IsAny<CSVExportRequestArgument>(), It.IsAny<CancellationToken>())).Returns((CSVExportRequestComputeFunc func, CSVExportRequestArgument argument, CancellationToken token) => Task.FromResult<CSVExportRequestResponse>(null));

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be("Failed to complete TRex request: CSVExportExecutor with error: Failed to configure internal pipeline.");
    }

    [Fact]
    public async Task CSVExportExecutor_GotFilterWithContributingMachines()
    {
      var fileName = "gotAFilename";
      var coordType = CoordType.Northeast;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = { "first machineName" };
      var userPreferences = new UserPreferences();
      var contributingMachines = new List<MachineDetails>()
        { new MachineDetails(Consts.NULL_LEGACY_ASSETID, "Machine Name", false, Guid.NewGuid())};
      var filter = new Productivity3D.Filter.Abstractions.Models.Filter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc), "", "",
        contributingMachines, null, ElevationType.First, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null, null);
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CompactionVetaExportRequest(
        siteModel.ID, filterResult, fileName,
        coordType, outputType, userPreferences, machineNames, null, null);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      // Mock the CSV export request functionality to return a null CSV export reponse to stimulate tyhe desired internal processing error
      var mockCompute = IgniteMock.Immutable.mockCompute;
      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<CSVExportRequestComputeFunc>(), It.IsAny<CSVExportRequestArgument>(), It.IsAny<CancellationToken>())).Returns((CSVExportRequestComputeFunc func, CSVExportRequestArgument argument, CancellationToken token) => Task.FromResult<CSVExportRequestResponse>(null));

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be("Failed to complete TRex request: CSVExportExecutor with error: Failed to configure internal pipeline.");
    }
  }
}



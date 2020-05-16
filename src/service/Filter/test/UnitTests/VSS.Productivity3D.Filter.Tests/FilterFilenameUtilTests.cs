using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Filter.Repository;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;
using FilterModel = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Tests
{
  public class FilterFilenameUtilTests : IClassFixture<ExecutorBaseTests>
  {
    private readonly ExecutorBaseTests _classFixture;
    private IServiceProvider serviceProvider => _classFixture.serviceProvider;
    private IServiceExceptionHandler serviceExceptionHandler => _classFixture.serviceExceptionHandler;

    private readonly string custUid = Guid.NewGuid().ToString();
    private readonly string userUid = Guid.NewGuid().ToString();
    private readonly string projectUid = Guid.NewGuid().ToString();

    public FilterFilenameUtilTests(ExecutorBaseTests classFixture)
    {
      _classFixture = classFixture;
    }



    [Fact]
    public async Task Should_return_when_DesignUid_and_AlignmentUid_arent_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776}";

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var getMachinesExecutionResult = new MachineExecutionResult(new List<MachineStatus>(0));
      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(getMachinesExecutionResult);

      var filter = new FilterModel
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = FilterType.Persistent,
      };

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var filterRepo = new Mock<FilterRepository>(configStore, logger);

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new List<FilterModel>());
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUID = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.Null(resultFilter.AlignmentFileName);
      Assert.Null(resultFilter.DesignFileName);
    }

    [Fact]
    public async Task Should_return_When_no_files_exist_for_project()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"alignmentUid\":\"6ece671b-7959-4a14-86fa-6bfe6ef4dd62\"}";

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var getMachinesExecutionResult = new MachineExecutionResult(new List<MachineStatus>(0));
      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(getMachinesExecutionResult);

      var filter = new FilterModel
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = FilterType.Persistent,
      };

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var filterRepo = new Mock<FilterRepository>(configStore, logger);

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new List<FilterModel>());
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var fileImportProxy = new Mock<IFileImportProxy>();
      fileImportProxy.As<IFileImportProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(new List<FileData>());

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUID = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object,
        fileImportProxy: fileImportProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.Null(resultFilter.AlignmentFileName);
      Assert.Null(resultFilter.DesignFileName);
    }

    [Fact]
    public async Task Should_set_DesignName_When_DesignUid_is_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var getMachinesExecutionResult = new MachineExecutionResult(new List<MachineStatus>(0));
      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(getMachinesExecutionResult);

      var filter = new FilterModel
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = FilterType.Persistent,
      };

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var filterRepo = new Mock<FilterRepository>(configStore, logger);

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new List<FilterModel>());
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var fileData = new List<FileData>
      {
        new FileData
        {
          Name = "Large Sites Road - Trimble Road.TTM",
          ProjectUid = projectUid,
          CustomerUid = "CutFillAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
          LegacyFileId = 111,
          IsActivated = true

        }};

      var fileImportProxy = new Mock<IFileImportProxy>();
      fileImportProxy.As<IFileImportProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(fileData);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUID = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object,
        fileImportProxy: fileImportProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.Equal("Large Sites Road - Trimble Road.TTM", resultFilter.DesignFileName);
      Assert.Null(resultFilter.AlignmentFileName);
    }

    [Fact]
    public async Task Should_set_AlignmentName_When_AlignmentUid_is_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"alignmentUid\":\"6ece671b-7959-4a14-86fa-6bfe6ef4dd62\"}";

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var getMachinesExecutionResult = new MachineExecutionResult(new List<MachineStatus>(0));
      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(getMachinesExecutionResult);

      var filter = new FilterModel
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = FilterType.Persistent,
      };

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var filterRepo = new Mock<FilterRepository>(configStore, logger);

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new List<FilterModel>());
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var fileData = new List<FileData>
      {
        new FileData
        {
          Name = "Large Sites Road.svl",
          ProjectUid = projectUid,
          CustomerUid = "StationOffsetReportTest",
          ImportedFileType = ImportedFileType.Alignment,
          ImportedFileUid = "6ece671b-7959-4a14-86fa-6bfe6ef4dd62",
          LegacyFileId = 112,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 17
        }};

      var fileImportProxy = new Mock<IFileImportProxy>();
      fileImportProxy.As<IFileImportProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(fileData);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUID = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object,
        fileImportProxy: fileImportProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.Equal("Large Sites Road.svl", resultFilter.AlignmentFileName);
      Assert.Null(resultFilter.DesignFileName);
    }

    [Fact]
    public async Task Should_set_Alignment_and_Design_name_When_both_uids_are_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"alignmentUid\":\"6ece671b-7959-4a14-86fa-6bfe6ef4dd62\",\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var getMachinesExecutionResult = new MachineExecutionResult(new List<MachineStatus>(0));
      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(x =>
          x.ExecuteGenericV2Request<MachineExecutionResult>(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(), It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(getMachinesExecutionResult);

      var filter = new FilterModel
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = FilterType.Persistent,
      };

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var filterRepo = new Mock<FilterRepository>(configStore, logger);

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new List<FilterModel>());
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var fileData = new List<FileData>
      {
        new FileData
        {
          Name = "Large Sites Road.svl",
          ProjectUid = projectUid,
          CustomerUid = "StationOffsetReportTest",
          ImportedFileType = ImportedFileType.Alignment,
          ImportedFileUid = "6ece671b-7959-4a14-86fa-6bfe6ef4dd62",
          LegacyFileId = 112,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 17
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road.TTM",
          ProjectUid = projectUid,
          CustomerUid = "CutFillAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
          LegacyFileId = 111,
          IsActivated = true
        }};

      var fileImportProxy = new Mock<IFileImportProxy>();
      fileImportProxy.As<IFileImportProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(fileData);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUID = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object,
        fileImportProxy: fileImportProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.Equal("Large Sites Road.svl", resultFilter.AlignmentFileName);
      Assert.Equal("Large Sites Road - Trimble Road.TTM", resultFilter.DesignFileName);
    }
  }
}

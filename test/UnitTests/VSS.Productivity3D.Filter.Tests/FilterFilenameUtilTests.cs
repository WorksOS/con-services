using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using FilterModel = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterFilenameUtilTests : ExecutorBaseTests
  {
    private readonly string custUid = Guid.NewGuid().ToString();
    private readonly string userUid = Guid.NewGuid().ToString();
    private readonly string projectUid = Guid.NewGuid().ToString();
    private string KafkaTopicName => GetType().Name;
    private static Mock<IKafka> Producer => new Mock<IKafka>();

    [TestMethod]
    public async Task Should_return_when_DesignUid_and_AlignmentUid_arent_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776}";

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object, null, raptorProxy.Object, Producer.Object, KafkaTopicName);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.IsNull(resultFilter.AlignmentName);
      Assert.IsNull(resultFilter.DesignName);
    }

    [TestMethod]
    public async Task Should_return_When_no_files_exist_for_project()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"alignmentUid\":\"6ece671b-7959-4a14-86fa-6bfe6ef4dd62\"}";

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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

      var fileListProxy = new Mock<IFileListProxy>();
      fileListProxy.As<IFileListProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new List<FileData>());

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object, null, raptorProxy.Object, Producer.Object, KafkaTopicName, fileListProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.IsNull(resultFilter.AlignmentName);
      Assert.IsNull(resultFilter.DesignName);
    }

    [TestMethod]
    public async Task Should_set_DesignName_When_DesignUid_is_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 20
        }};

      var fileListProxy = new Mock<IFileListProxy>();
      fileListProxy.As<IFileListProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(fileData);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object, null, raptorProxy.Object, Producer.Object, KafkaTopicName, fileListProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.AreEqual(resultFilter.DesignName, "Large Sites Road - Trimble Road.TTM");
      Assert.IsNull(resultFilter.AlignmentName);
    }

    [TestMethod]
    public async Task Should_set_AlignmentName_When_AlignmentUid_is_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"alignmentUid\":\"6ece671b-7959-4a14-86fa-6bfe6ef4dd62\"}";

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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

      var fileListProxy = new Mock<IFileListProxy>();
      fileListProxy.As<IFileListProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(fileData);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object, null, raptorProxy.Object, Producer.Object, KafkaTopicName, fileListProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.AreEqual(resultFilter.AlignmentName, "Large Sites Road.svl");
      Assert.IsNull(resultFilter.DesignName);
    }

    [TestMethod]
    public async Task Should_set_Alignment_and_Design_name_When_both_uids_are_provided()
    {
      var name = Guid.NewGuid().ToString();
      const string filterJson = "{\"startUtc\":\"2012-10-30T00:12:09.109\",\"endUtc\":\"2018-06-14T11:58:13.662\",\"dateRangeType\":7,\"contributingMachines\":[{\"assetID\":\"751877972662699\",\"machineName\":\"KOMATSU PC210\",\"isJohnDoe\":false}],\"onMachineDesignId\":\"1\",\"startStation\":0.0,\"endStation\":197.7762153912619,\"leftOffset\":0.0,\"rightOffset\":197.776,\"alignmentUid\":\"6ece671b-7959-4a14-86fa-6bfe6ef4dd62\",\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 20
        }};

      var fileListProxy = new Mock<IFileListProxy>();
      fileListProxy.As<IFileListProxy>().Setup(ps => ps.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(fileData);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create(
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = null, Name = name, FilterJson = filterJson, FilterType = FilterType.Persistent });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object, null, raptorProxy.Object, Producer.Object, KafkaTopicName, fileListProxy.Object);

      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

      var resultFilter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(result.FilterDescriptor.FilterJson);
      Assert.AreEqual(resultFilter.AlignmentName, "Large Sites Road.svl");
      Assert.AreEqual(resultFilter.DesignName, "Large Sites Road - Trimble Road.TTM");
    }
  }
}

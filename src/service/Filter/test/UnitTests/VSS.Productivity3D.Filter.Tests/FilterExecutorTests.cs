using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Extensions;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class FilterExecutorTests : IClassFixture<ExecutorBaseTests>
  {
    private readonly ExecutorBaseTests _classFixture;
    private IServiceProvider serviceProvider => _classFixture.serviceProvider;
    private IServiceExceptionHandler serviceExceptionHandler => _classFixture.serviceExceptionHandler;

    public FilterExecutorTests(ExecutorBaseTests classFixture)
    {
      _classFixture = classFixture;
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public async Task GetFilterExecutor(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = "{\"dateRangeType\":\"0\",\"elevationType\":null}",
        FilterType = filterType,
        LastActionedUtc = DateTime.UtcNow
      };
      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFilter(It.IsAny<string>())).ReturnsAsync(filter);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create
      (
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid }
      );

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", result.FilterDescriptor.FilterJson);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public async Task GetFilterExecutor_DoesntBelongToUser(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string requestingUserUid = Guid.NewGuid().ToString();
      string filterUserUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "{{\"dateRangeType\":\"0\",\"elevationType\":null}}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = custUid,
        UserId = filterUserUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = filterType,
        LastActionedUtc = DateTime.UtcNow
      };

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFilter(It.IsAny<string>())).ReturnsAsync(filter);

      var request = FilterRequestFull.Create
      (
        null,
        custUid,
        false,
        requestingUserUid,
        new ProjectData { ProjectUid = projectUid },
        new FilterRequest { FilterUid = filterUid }
      );
      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object, null);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(request));

      Assert.Contains("2036", ex.GetContent);
      Assert.Contains("GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.", ex.GetContent);
    }

    [Fact]
    public async Task GetFiltersExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filters = new List<MasterData.Repositories.DBModels.Filter>
      {
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = "{\"dateRangeType\":\"0\",\"elevationType\":null}",
          FilterType = FilterType.Persistent,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(filters);

      var filterListToTest = new FilterDescriptorListResult
      {
        FilterDescriptors = filters
          .Select(filter => AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
          .ToImmutableList()
      };

      var request = FilterRequestFull.Create
      (
        null,
        custUid,
        false,
        userUid,
        new ProjectData { ProjectUid = projectUid });

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object, null);
      var filterListResult = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      Assert.NotNull(filterListResult);
      Assert.Equal(filterListToTest.FilterDescriptors[0].FilterUid, filterListResult.FilterDescriptors[0].FilterUid);
      Assert.Equal(filterListToTest.FilterDescriptors[0].Name, filterListResult.FilterDescriptors[0].Name);
      Assert.Equal("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", filterListResult.FilterDescriptors[0].FilterJson);
      Assert.Equal(filterListToTest.FilterDescriptors[0].FilterType, filterListResult.FilterDescriptors[0].FilterType);
    }

    [Fact]
    public async Task UpsertFilterExecutor_Transient()
    {
      // this scenario, the FilterUid is supplied, this should throw an exception as update not supported.
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = string.Empty;
      string filterJson = "{\"designUID\": \"id\", \"vibeStateOn\": true}";
      FilterType filterType = FilterType.Transient;

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var projectProxy = new Mock<IProjectProxy>();
      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filters = new List<MasterData.Repositories.DBModels.Filter>
      {
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          FilterType = filterType,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(filters);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);

      var request =
        FilterRequestFull.Create
        (
          null,
          custUid,
          false,
          userUid,
          new ProjectData { ProjectUid = projectUid },
          new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJson, FilterType = filterType }
        );
      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object, projectProxy.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, producer: producer.Object, kafkaTopicName: kafkaTopicName);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(request));

      Assert.Contains("2016", ex.GetContent);
      Assert.Contains("UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.", ex.GetContent);
    }

    [Fact]
    public async Task UpsertFilterExecutor_TransientFilterWithBoundary()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string boundaryUid = Guid.NewGuid().ToString();
      string filterJson = "{\"designUid\": \"id\", \"vibeStateOn\": true, \"polygonUid\": \"" + boundaryUid + "\"}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filters = new List<MasterData.Repositories.DBModels.Filter>
      {
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = string.Empty,
          FilterJson = filterJson,
          FilterType = FilterType.Transient,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(filters);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);
      var geofence = new Geofence
      {
        GeofenceUID = boundaryUid,
        Name = "whatever",
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        GeofenceType = GeofenceType.Filter,
        CustomerUID = custUid,
        UserUID = userUid,
        LastActionedUTC = DateTime.UtcNow
      };
      geofenceRepo.As<IGeofenceRepository>().Setup(g => g.GetGeofence(It.IsAny<string>())).ReturnsAsync(geofence);

      var request =
        FilterRequestFull.Create
        (
          null,
          custUid,
          false,
          userUid,
          new ProjectData { ProjectUid = projectUid },
          new FilterRequest { Name = string.Empty, FilterJson = filterJson, FilterType = FilterType.Transient }
        );

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, producer: producer.Object, kafkaTopicName: kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      //Actual filter UID can not be validated as it is randomly generated by the logic. INstead we can check if it is not empty
      //Assert.Equal(filterUid, result.FilterDescriptor.FilterUid, "Wrong filterUid");
      Assert.False(string.IsNullOrEmpty(result.FilterDescriptor.FilterUid));
      //Because of mocking can't use result JSON but request JSON should be hydrated
      var boundary = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(/*result.FilterDescriptor.FilterJson*/request.FilterJson);
      Assert.Equal(geofence.GeofenceUID, boundary.PolygonUid);
      Assert.Equal(geofence.Name, boundary.PolygonName);
      Assert.Equal(geofence.GeometryWKT, GetWicketFromPoints(boundary.PolygonLL));
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent(FilterType filterType)
    {
      // this scenario, the FilterUid is supplied, and is provided in Request
      // so this will result in an updated filter
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string filterJson = "{\"vibeStateOn\":true}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = filterType,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>
      {
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          FilterType = filterType,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(filters);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.Create(
          null,
          custUid,
          false,
          userUid,
          new ProjectData { ProjectUid = projectUid },
          new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJson, FilterType = filterType });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, producer: producer.Object, kafkaTopicName: kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      Assert.Equal(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid);
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterJson, result.FilterDescriptor.FilterJson);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);
    }

    [Fact]
    public async Task UpsertFilterExecutor_Persistent_WithCombiningWidgetFilters_CreateOnly()
    {
      // this scenario, the FilterUid is supplied, and is provided in Request
      // so this will result in an updated filter
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name = "not entry";

      string designUid = Guid.NewGuid().ToString();
      string designName = $"Name-{designUid}";

      string startVolumeDate = DateTime.UtcNow.AddHours(-1).ToIso8601DateTimeString();
      string endVolumeDate = DateTime.UtcNow.ToIso8601DateTimeString();

      string filterUid = Guid.NewGuid().ToString();
      string filterJson = "{\"vibeStateOn\":\"false\", \"designName\":\"" + designName + "\", \"designUid\":\"" + designUid + "\", \"dateRangeType\":\"Custom\", \"startUTC\": \"" + startVolumeDate + "\", \"endUTC\":\"" + endVolumeDate + "\"}";

      string filterUid_Master = Guid.NewGuid().ToString();
      string filterJson_Master = "{\"vibeStateOn\":true}";

      string filterUid_Widget = Guid.NewGuid().ToString();
      string filterJson_Widget = "{\"vibeStateOn\":false}";

      string filterUid_Volume = Guid.NewGuid().ToString();
      string filterJson_Volume = "{\"designUid\":\"" + designUid + "\", \"dateRangeType\":\"Custom\", \"startUTC\": \"" + startVolumeDate+"\", \"endUTC\":\"" + endVolumeDate +"\"}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var fileImportProxy = new Mock<IFileImportProxy>();
      fileImportProxy.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), null)).ReturnsAsync
        (new List<FileData>
      {
        new FileData { CustomerUid = custUid, ProjectUid = projectUid, Name = designName, ImportedFileUid = designUid }
      });

      var producer = new Mock<IKafka>();
      var kafkaTopicName = "whatever";

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = FilterType.Widget,
        LastActionedUtc = DateTime.UtcNow
      };

      var filters = new List<MasterData.Repositories.DBModels.Filter>
      {
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid_Master,
          Name = name,
          FilterJson = filterJson_Master,
          FilterType = FilterType.Widget,
          LastActionedUtc = DateTime.UtcNow
        },
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid_Widget,
          Name = name,
          FilterJson = filterJson_Widget,
          FilterType = FilterType.Widget,
          LastActionedUtc = DateTime.UtcNow
        },
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid_Volume,
          Name = name,
          FilterJson = filterJson_Volume,
          FilterType = FilterType.Widget,
          LastActionedUtc = DateTime.UtcNow
        }
      };

      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(filters);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var geofenceRepo = new Mock<GeofenceRepository>(configStore, logger);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.Create(
          null,
          custUid,
          false,
          userUid,
          new ProjectData { ProjectUid = projectUid },
          new FilterRequest
          {
            Name = name, 
            FilterJson = string.Empty, 
            FilterType = FilterType.Widget,
            FilterUids = new List<CombineFiltersRequestElement>
            {
              new CombineFiltersRequestElement { FilterUid = filterUid_Master, Role = FilterCombinationRole.MasterFilter },
              new CombineFiltersRequestElement { FilterUid = filterUid_Widget, Role = FilterCombinationRole.WidgetFilter },
              new CombineFiltersRequestElement { FilterUid = filterUid_Volume, Role = FilterCombinationRole.VolumesFilter }
            }
          });

      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, geofenceRepo.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, 
        producer: producer.Object, kafkaTopicName: kafkaTopicName,
        fileImportProxy:fileImportProxy.Object);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.NotNull(result);
      result.FilterDescriptor.FilterUid.Should().NotBeNullOrEmpty();
      Assert.Equal(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name);
      Assert.Equal(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType);

       var testFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterToTest.FilterDescriptor.FilterJson);
       var combinedFilter = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(result.FilterDescriptor.FilterJson);

       testFilter.Should().BeEquivalentTo(combinedFilter);
    }

    [Theory]
    [InlineData(FilterType.Persistent)]
    [InlineData(FilterType.Report)]
    [InlineData(FilterType.Widget)]
    public async Task DeleteFilterExecutor(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var projectProxy = new Mock<IProjectProxy>();
      var productivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      productivity3dV2ProxyNotification.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseMasterDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<FilterRepository>(configStore, logger);
      var filters = new List<MasterData.Repositories.DBModels.Filter>
      {
        new MasterData.Repositories.DBModels.Filter
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          FilterType = filterType,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.As<IFilterRepository>().Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(filters);
      filterRepo.As<IFilterRepository>().Setup(ps => ps.StoreEvent(It.IsAny<DeleteFilterEvent>())).ReturnsAsync(1);

      var request =
        FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJson, FilterType = filterType });
      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, null, projectProxy.Object,
        productivity3dV2ProxyNotification: productivity3dV2ProxyNotification.Object, producer: producer.Object, kafkaTopicName: kafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.NotNull(result);
      Assert.Equal(0, result.Code);
    }

    private string GetWicketFromPoints(List<WGSPoint> points)
    {
      if (points.Count == 0)
        return string.Empty;

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(string.Format("{0} {1},", point.Lon, point.Lat));
      }
      if (points[0] != points[points.Count - 1])
      {
        polygonWkt.Append(string.Format("{0} {1},", points[0].Lon, points[0].Lat));
      }
      return polygonWkt.ToString().TrimEnd(',') + "))";
    }
  }
}

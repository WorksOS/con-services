using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
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

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid,
        "executor returned incorrect FilterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name,
        "executor returned incorrect FilterDescriptor Name");
      Assert.AreEqual("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", result.FilterDescriptor.FilterJson,
        "executor returned incorrect FilterDescriptor FilterJson");
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType,
        "executor returned incorrect FilterDescriptor FilterType");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
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
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2036");
      StringAssert.Contains(ex.GetContent, "GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.");
    }

    [TestMethod]
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
        FilterDescriptors = filters.Select(filter => AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
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

      Assert.IsNotNull(filterListResult, "executor failed");
      Assert.AreEqual(filterListToTest.FilterDescriptors[0].FilterUid, filterListResult.FilterDescriptors[0].FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterListToTest.FilterDescriptors[0].Name, filterListResult.FilterDescriptors[0].Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}",
        filterListResult.FilterDescriptors[0].FilterJson, "executor returned incorrect filterDescriptor FilterJson");
      Assert.AreEqual(filterListToTest.FilterDescriptors[0].FilterType, filterListResult.FilterDescriptors[0].FilterType,
        "executor returned incorrect filterDescriptor FilterType");
    }

    [TestMethod]
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

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
        filterRepo.Object, geofenceRepo.Object, projectListProxy.Object, 
        raptorProxy.Object, producer.Object, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2016");
      StringAssert.Contains(ex.GetContent, "UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.");
    }

    [TestMethod]
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

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
        filterRepo.Object, geofenceRepo.Object, null,
        raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;
      
      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code, "executor returned incorrect result code");
      //Actual filter UID can not be validated as it is randomly generated by the logic. INstead we can check if it is not empty
      //Assert.AreEqual(filterUid, result.FilterDescriptor.FilterUid, "Wrong filterUid");
      Assert.IsFalse(String.IsNullOrEmpty(result.FilterDescriptor.FilterUid));
      //Because of mocking can't use result JSON but request JSON should be hydrated
      var boundary = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(/*result.FilterDescriptor.FilterJson*/request.FilterJson);
      Assert.AreEqual(geofence.GeofenceUID, boundary.PolygonUid, "Wrong polygonUID");
      Assert.AreEqual(geofence.Name, boundary.PolygonName, "Wrong polygonName");
      Assert.AreEqual(geofence.GeometryWKT, GetWicketFromPoints(boundary.PolygonLL), "Wrong polygonLL");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
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

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
        filterRepo.Object, geofenceRepo.Object, null, 
        raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid,
        "executor returned incorrect FilterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name,
        "executor returned incorrect FilterDescriptor Name");
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterJson, result.FilterDescriptor.FilterJson,
        "executor returned incorrect FilterDescriptor FilterJson");
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType,
        "executor returned incorrect FilterDescriptor FilterType");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
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

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

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
        filterRepo.Object, null, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
    }

    private string GetWicketFromPoints(List<WGSPoint> points)
    {
      if (points.Count == 0)
        return string.Empty;

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.Lon, point.Lat));
      }
      if (points[0] != points[points.Count - 1])
      {
        polygonWkt.Append(String.Format("{0} {1},", points[0].Lon, points[0].Lat));
      }
      return polygonWkt.ToString().TrimEnd(',') + "))";
    }
  }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task GetFilterExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      filterRepo.Setup(ps => ps.GetFilter(It.IsAny<string>())).ReturnsAsync(filter);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create
      (
        customerUid: custUid, 
        isApplicationContext: false,
        userId: userUid, 
        projectUid: projectUid,
        
        filterUid: filterUid
      );
      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task GetFilterExecutor_DoesntBelongToUser()
    {
      string custUid = Guid.NewGuid().ToString();
      string requestingUserUid = Guid.NewGuid().ToString();
      string filterUserUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserId = filterUserUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      filterRepo.Setup(ps => ps.GetFilter(It.IsAny<string>())).ReturnsAsync(filter);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.Create
      (
        customerUid: custUid,
        isApplicationContext: false,
        userId: requestingUserUid,
        projectUid: projectUid,

        filterUid: filterUid
      );
      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2036");
      StringAssert.Contains(ex.GetContent, "GetFilter By filterUid. The requested filter does exist, or does not belong to the requesting customer; project or user.");
    }

    [TestMethod]
    public async Task GetFiltersExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var filterRepo = new Mock<IFilterRepository>();
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(filters);

      var filterListToTest = new FilterDescriptorListResult
      {
        filterDescriptors = filters.Select(filter =>
            AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
          .ToImmutableList()
      };

      var request = FilterRequestFull.Create
      (
        customerUid: custUid,
        isApplicationContext: false,
        userId: userUid,
        projectUid: projectUid
      );
      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object);
      var filterListResult = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      Assert.IsNotNull(filterListResult, "executor failed");
      Assert.AreEqual(filterListToTest.filterDescriptors[0], filterListResult.filterDescriptors[0],
        "executor returned incorrect filterDescriptor");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].FilterUid, filterListResult.filterDescriptors[0].FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].Name, filterListResult.filterDescriptors[0].Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].FilterJson,
        filterListResult.filterDescriptors[0].FilterJson, "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient()
    {
      // this scenario, the filterUid is supplied, this should throw an exception as update not supported.
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = string.Empty;
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(filters);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.Create
        (
          customerUid: custUid,
          isApplicationContext: false,
          userId: userUid,
          projectUid: projectUid,
          filterUid: filterUid,
          name: name,
          filterJson: filterJson
        );
      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2016");
      StringAssert.Contains(ex.GetContent, "UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Persistent()
    {
      // this scenario, the filterUid is supplied, and is provided in request
      // so this will result in an updated filter
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(filters);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task DeleteFilterExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(filters);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<DeleteFilterEvent>())).ReturnsAsync(1);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
    }

    [TestMethod]
    public async Task CreateFiltersExecutor_3GoodFilters()
    {
      // a list of 3 valid transient filters are sent in request to creat
      // a list of 3 should be returned
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string filterUid3 = Guid.NewGuid().ToString();
      string filterJson1 = "";
      string filterJson2 = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}";
      string filterJson3 = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      // request data:
      var requestList = new List<FilterRequest>()
      {
        FilterRequest.Create("", "", filterJson1),
        FilterRequest.Create("", "", filterJson2),
        FilterRequest.Create("", "", filterJson3)
      };

      FilterListRequest filterListRequest = new FilterListRequest()
      {
        filterRequests = new List<FilterRequest>()
      };
      filterListRequest.filterRequests = requestList.ToImmutableList();

      var filterListRequestFull = new FilterListRequestFull()
      {
        CustomerUid = custUid,
        ProjectUid = projectUid,
        filterRequests = filterListRequest.filterRequests
      };
      //// expected result data:
      //var resultList = new List<FilterDescriptor>()
      //{
      //  new FilterDescriptor(){ FilterUid = filterUid1, Name = name, FilterJson = filterJson1}
      //};
      //var expectedResultList = new FilterDescriptorListResult {filterDescriptors = resultList.ToImmutableList()};


      // setup moq
      var filterRepo = new Mock<IFilterRepository>();
      //var dbGetResult = new MasterData.Repositories.DBModels.Filter()
      //{
      //  CustomerUid = custUid,
      //  UserId = userUid,
      //  ProjectUid = projectUid,
      //  FilterUid = filterUid1,
      //  Name = "",
      //  FilterJson = filterJson1,
      //  LastActionedUtc = DateTime.UtcNow
      //};

      var dbGetResultList1 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid1,
          Name = "",
          FilterJson = filterJson1,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      var dbGetResultList2 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid2,
          Name = "",
          FilterJson = filterJson2,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      var dbGetResultList3 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid3,
          Name = "",
          FilterJson = filterJson3,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);
      filterRepo.SetupSequence(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
        .ReturnsAsync(dbGetResultList1)
        .ReturnsAsync(dbGetResultList2)
        .ReturnsAsync(dbGetResultList3);

      var executor = RequestExecutorContainer.Build<CreateFiltersExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object);
      var result = await executor.ProcessAsync(filterListRequestFull) as FilterDescriptorListResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(3, result.filterDescriptors.Count, "Wrong result count returned");
      Assert.AreNotEqual("", result.filterDescriptors[0].FilterUid, "first filterUid incorrect");
      Assert.AreEqual(filterJson1, result.filterDescriptors[0].FilterJson, "first filterJson incorrect");
      Assert.AreEqual(filterJson2, result.filterDescriptors[1].FilterJson, "second filterJson incorrect");
      Assert.AreEqual(filterJson3, result.filterDescriptors[2].FilterJson, "third filterJson incorrect");
    }

    [TestMethod]
    public async Task CreateFiltersExecutor_2GoodFilters_1BadRead()
    {
      // a list of 3 valid and 1 invalid (fails to read) transient filters are sent in request to create
      // should throw exception
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string filterUid3 = Guid.NewGuid().ToString();
      string filterJson1 = "";
      string filterJson2 = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}";
      string filterJson3 = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      // request data:
      var requestList = new List<FilterRequest>()
      {
        FilterRequest.Create("", "", filterJson1),
        FilterRequest.Create("", "", filterJson2),
        FilterRequest.Create("", "", filterJson3)
      };

      FilterListRequest filterListRequest = new FilterListRequest()
      {
        filterRequests = new List<FilterRequest>()
      };
      filterListRequest.filterRequests = requestList.ToImmutableList();

      var filterListRequestFull = new FilterListRequestFull()
      {
        CustomerUid = custUid,
        ProjectUid = projectUid,
        filterRequests = filterListRequest.filterRequests
      };

      // setup moq
      var filterRepo = new Mock<IFilterRepository>();

      var dbGetResultList1 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid1,
          Name = "",
          FilterJson = filterJson1,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      var dbGetResultList2 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid2,
          Name = "",
          FilterJson = filterJson2,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      var dbGetResultList3 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid3,
          Name = "",
          FilterJson = filterJson3,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.SetupSequence(
          ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
        .ReturnsAsync(dbGetResultList1)
        .ReturnsAsync(dbGetResultList2);
      // third missing;
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);


      var executor = RequestExecutorContainer.Build<CreateFiltersExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(filterListRequestFull).ConfigureAwait(false)); ;

      StringAssert.Contains(ex.GetContent, "2019");
      StringAssert.Contains(ex.GetContent, "UpsertFilter failed. Unable to create transient filter.");
    }

    [TestMethod]
    public async Task CreateFiltersExecutor_2GoodFilters_1BadStore()
    {
      // a list of 3 valid and 1 invalid (bad store) transient filters are sent in request to create
      // should throw exception
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string filterUid3 = Guid.NewGuid().ToString();
      string filterJson1 = "";
      string filterJson2 = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}";
      string filterJson3 = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      // request data:
      var requestList = new List<FilterRequest>()
      {
        FilterRequest.Create("", "", filterJson1),
        FilterRequest.Create("", "", filterJson2),
        FilterRequest.Create("", "", filterJson3)
      };

      FilterListRequest filterListRequest = new FilterListRequest()
      {
        filterRequests = new List<FilterRequest>()
      };
      filterListRequest.filterRequests = requestList.ToImmutableList();

      var filterListRequestFull = new FilterListRequestFull()
      {
        CustomerUid = custUid,
        ProjectUid = projectUid,
        filterRequests = filterListRequest.filterRequests
      };

      // setup moq
      var filterRepo = new Mock<IFilterRepository>();

      var dbGetResultList1 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid1,
          Name = "",
          FilterJson = filterJson1,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      var dbGetResultList2 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid2,
          Name = "",
          FilterJson = filterJson2,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      var dbGetResultList3 = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserId = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid3,
          Name = "",
          FilterJson = filterJson3,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.SetupSequence(
          ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
        .ReturnsAsync(dbGetResultList1)
        .ReturnsAsync(dbGetResultList2)
        .ReturnsAsync(dbGetResultList3);
      filterRepo.SetupSequence(
          ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>()))
        .ReturnsAsync(1)
        .ReturnsAsync(1)
        .ReturnsAsync(0);

      var executor = RequestExecutorContainer.Build<CreateFiltersExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(filterListRequestFull).ConfigureAwait(false)); ;

      StringAssert.Contains(ex.GetContent, "2019");
      StringAssert.Contains(ex.GetContent, "UpsertFilter failed. Unable to create transient filter.");
    }
  }
}


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ValidationTests : ExecutorBaseTests
  {
    private readonly string custUid = Guid.NewGuid().ToString();
    private readonly string userUid = Guid.NewGuid().ToString();
    private readonly string projectUid = Guid.NewGuid().ToString();
    private readonly string filterUid = Guid.NewGuid().ToString();
    private const string Name = "blah";
    private const string FilterJson = "theJsonString";

    private IServiceExceptionHandler _serviceExceptionHandler;

    [TestInitialize]
    public void Initialize()
    {
      _serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }

    [TestMethod]
    public void FilterRequestValidation_MissingCustomerUid()
    {
      var requestFull =
        FilterRequestFull.Create
        (
          "sfgsdfsf",
          false,
          userUid,
          projectUid,
          filterUid,
          Name,
          FilterJson
        );
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2027");
      StringAssert.Contains(ex.GetContent, "Invalid customerUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_MissingUserId()
    {
      var requestFull =
        FilterRequestFull.Create
        (
          custUid, 
          false, 
          "", 
          projectUid, 
          filterUid, 
          Name, 
          FilterJson
        );
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2028");
      StringAssert.Contains(ex.GetContent, "Invalid userUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_MissingProjectUid()
    {
      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, null, filterUid, Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, "this is so wrong", Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid_Null()
    {
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, null, Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, null, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2003");
      StringAssert.Contains(ex.GetContent, "Invalid name. Should not be null.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, Name, null);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2004");
      StringAssert.Contains(ex.GetContent, "Invalid filterJson. Should not be null.");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("{ \"filterUid\": \"00000000-0000-0000-0000-000000000000\" }")]
    public void FilterRequestValidation_Should_succeed_When_supplied_json_is_valid(string filterJson)
    {
      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, "", filterJson);
      requestFull.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_Should_fail_When_supplied_string_is_invalid_json()
    {
      var requestFull = FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, Name, "de blah");
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2042");
      StringAssert.Contains(ex.GetContent, "Invalid filterJson. Exception: Unexpected character encountered while parsing value:");
    }

    [TestMethod]
    public void FilterRequestValidation_PartialFill()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.Create(custUid, false, userUid, projectUid, "");
      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_HappyPath()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>
      {
        new MasterData.Models.Models.ProjectData {ProjectUid = projectUid, CustomerUid = custUid}
      };
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      await FilterValidation.ValidateProjectForCustomer(projectListProxy.Object, log, _serviceExceptionHandler,
        customHeaders, custUid, projectUid).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_NoAssociation()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>();
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await FilterValidation.ValidateProjectForCustomer(projectListProxy.Object, log, _serviceExceptionHandler,
        customHeaders, custUid, projectUid).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2008");
      StringAssert.Contains(ex.GetContent, "Validation of Customer/Project failed. Not allowed.");
    }

    [TestMethod]
    public async Task CreateFiltersValidation_PersistantNotAllowed()
    {
      // a list of 3 valid transient filters are sent in request to creat
      // a list of 3 should be returned
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name1 = "";
      string name2 = "This is persistant";
      string name3 = "";
      string filterJson1 = "";
      string filterJson2 = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}";
      string filterJson3 = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"}";

      // request data:
      var requestList = new List<FilterRequest>()
      {
        FilterRequest.Create("", name1, filterJson1),
        FilterRequest.Create("", name2, filterJson2),
        FilterRequest.Create("", name3, filterJson3)
      };

      FilterListRequest filterListRequest = new FilterListRequest()
      {
        filterRequests = new List<FilterRequest>()
      };
      filterListRequest.filterRequests = requestList.ToImmutableList();

      var filterListRequestFull = new FilterListRequestFull()
      {
        CustomerUid = custUid,
        UserId = userUid,
        ProjectUid = projectUid,
        filterRequests = filterListRequest.filterRequests
      };

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var ex = Assert.ThrowsException<ServiceException>(() => filterListRequestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2024");
      StringAssert.Contains(ex.GetContent, "UpsertFilter failed. Unable to create persistent filter.");
    }
  }
}
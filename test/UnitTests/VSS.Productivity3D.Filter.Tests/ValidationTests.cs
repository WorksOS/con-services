using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
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
        FilterRequestFull.CreateFilterFullRequest("sfgsdfsf", false, userUid, projectUid, filterUid, Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2027");
      StringAssert.Contains(ex.GetContent, "Invalid customerUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_MissingUserUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, "", projectUid, filterUid, Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2028");
      StringAssert.Contains(ex.GetContent, "Invalid userUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_MissingProjectUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, null, filterUid, Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2001");
      StringAssert.Contains(ex.GetContent, "Invalid projectUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, "this is so wrong", Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid_EmptyString()
    {
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, "", Name, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2002");
      StringAssert.Contains(ex.GetContent, "Invalid filterUid.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, null, FilterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2003");
      StringAssert.Contains(ex.GetContent, "Invalid name. Should not be null.");
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, Name, null);
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
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, "", filterJson);
      requestFull.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_Should_fail_When_supplied_string_is_invalid_json()
    {
      var requestFull = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, Name, "de blah");
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      StringAssert.Contains(ex.GetContent, "2004");
      StringAssert.Contains(ex.GetContent, "Invalid filterJson");
    }

    [TestMethod]
    public void FilterRequestValidation_PartialFill()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid);
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

      await FilterValidation.ValidateCustomerProject(projectListProxy.Object, log, _serviceExceptionHandler,
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

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await FilterValidation.ValidateCustomerProject(projectListProxy.Object, log, _serviceExceptionHandler,
        customHeaders, custUid, projectUid).ConfigureAwait(false));

      StringAssert.Contains(ex.GetContent, "2008");
      StringAssert.Contains(ex.GetContent, "Validation of Customer/Project failed. Not allowed.");
    }
  }
}
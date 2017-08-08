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
    readonly string _custUid = Guid.NewGuid().ToString();
    readonly string _userUid = Guid.NewGuid().ToString();
    readonly string _projectUid = Guid.NewGuid().ToString();
    readonly string _filterUid = Guid.NewGuid().ToString();
    readonly string _name = "blah";
    readonly string _filterJson = "theJsonString";

    private static IServiceExceptionHandler _serviceExceptionHandler;

    [TestInitialize]
    public void Initialize()
    {
      _serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }
    
    [TestMethod]
    public void FilterRequestValidation_MissingCustomerUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest("sfgsdfsf", false, _userUid, _projectUid, _filterUid, _name, _filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2027"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid customerUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_MissingUserUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, "", _projectUid, _filterUid, _name, _filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2028"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid userUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_MissingProjectUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, null, _filterUid, _name, _filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2001"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid projectUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, _projectUid, "this is so wrong", _name, _filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2002"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid filterUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid_EmptyString()
    {
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, _projectUid, "", _name, _filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2002"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid filterUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, _projectUid, _filterUid, null, _filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2003"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid name. Should not be null.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, _projectUid, _filterUid, _name, null);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2004"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid filterJson. Should not be null.", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("{ \"filterUid\": \"00000000-0000-0000-0000-000000000000\" }")]
    public void FilterRequestValidation_Should_succeed_When_supplied_json_is_valid(string filterJson)
    {
      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, _projectUid, _filterUid, "", filterJson);
      requestFull.Validate(_serviceExceptionHandler);
    }

    [TestMethod]
    public void FilterRequestValidation_Should_fail_When_supplied_string_is_invalid_json()
    {
      var requestFull = FilterRequestFull.CreateFilterFullRequest(_custUid, false, _userUid, _projectUid, _filterUid, _name, "de blah");
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(_serviceExceptionHandler));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2004"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid filterJson", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task CustomerProjectValidation_HappyPath()
    {
      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>
      {
        new MasterData.Models.Models.ProjectData {ProjectUid = _projectUid, CustomerUid = _custUid}
      };
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      await FilterValidation.ValidateCustomerProject(projectListProxy.Object, log, _serviceExceptionHandler,
        customHeaders, _custUid, _projectUid).ConfigureAwait(false);
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
        customHeaders, _custUid, _projectUid).ConfigureAwait(false));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2008"));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Validation of Customer/Project failed. Not allowed.", StringComparison.Ordinal));
    }
  }
}
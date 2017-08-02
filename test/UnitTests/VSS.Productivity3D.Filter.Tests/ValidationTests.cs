using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Internal;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ValidationTests : ExecutorBaseTests
  {
    [TestMethod]
    public void FilterRequestValidation_MissingCustomerUid()
    {
      string custUid = "sfgsdfsf";
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid customerUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_MissingUserUid()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = "";
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid userUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_MissingProjectUid()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = null;
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid projectUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterUid()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = "this is so wrong";
      string name = "blah";
      string filterJson = "theJsonString";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid filterUid.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidName()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = null;
      string filterJson = "theJsonString";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid name. Should not be null.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_InvalidFilterJson()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = null;

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var ex = Assert.ThrowsException<ServiceException>(() => requestFull.Validate(serviceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Invalid filterJson. Should not be null.", StringComparison.Ordinal));
    }

    [TestMethod]
    public void FilterRequestValidation_emptyName()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      ;
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    // this should fail once Json validation is in place
    public void FilterRequestValidation_happyPath()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      ;
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "de blah";

      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var requestFull =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      requestFull.Validate(serviceExceptionHandler);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_HappyPath()
    {
      string custUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();
      ;
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>()
      {
        new MasterData.Models.Models.ProjectData() {ProjectUid = projectUid, CustomerUid = custUid}
      };
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      await FilterValidation.ValidateCustomerProject(projectListProxy.Object, log, serviceExceptionHandler,
        customHeaders, custUid, projectUid).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CustomerProjectValidation_NoAssociation()
    {
      string custUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ValidationTests>();;
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<MasterData.Models.Models.ProjectData>();
      var customHeaders = new Dictionary<string, string>();
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), customHeaders)).ReturnsAsync(projects);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await FilterValidation.ValidateCustomerProject(projectListProxy.Object, log, serviceExceptionHandler,
        customHeaders, custUid, projectUid).ConfigureAwait(false)); 
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Validation of Customer/Project failed. Not allowed.", StringComparison.Ordinal));
    }

  }
}


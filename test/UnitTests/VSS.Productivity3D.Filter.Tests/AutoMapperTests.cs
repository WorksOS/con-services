using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class AutoMapperTests
  {
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void MapDBModelToResult()
    {
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        ProjectUid = Guid.NewGuid().ToString(),
        FilterUid = Guid.NewGuid().ToString(),

        Name = "the Name",
        FilterJson = "the Json",

        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter);
      Assert.AreEqual(filter.FilterUid, result.FilterUid, "FilterUid has not been mapped correctly");
      Assert.AreEqual(filter.Name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(filter.FilterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToCreateKafkaEvent_UserContext()
    {
      var filterRequest = FilterRequestFull.CreateFilterFullRequest
      (
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the Name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.customerUid, result.CustomerUID.ToString(),
        "customerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.userId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.projectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "FilterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext()
    {
      var filterRequest = FilterRequestFull.CreateFilterFullRequest
      (
        Guid.NewGuid().ToString(),
        false,
        "ApplicationName",
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the Name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.customerUid, result.CustomerUID.ToString(),
        "customerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.userId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.projectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "FilterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToUpdateKafkaEvent()
    {
      var filterRequest = FilterRequestFull.CreateFilterFullRequest
      (
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the Name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<UpdateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.customerUid, result.CustomerUID.ToString(),
        "customerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.userId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.projectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "FilterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToDeleteKafkaEvent()
    {
      var filterRequest = FilterRequestFull.CreateFilterFullRequest
      (
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the Name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.customerUid, result.CustomerUID.ToString(),
        "customerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.userId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.projectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "FilterUid has not been mapped correctly");
    }

    [TestMethod]
    public void MapDBModelRequestToDeleteKafkaEvent()
    {
      var filter = new MasterData.Repositories.DBModels.Filter
      {
        CustomerUid = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        ProjectUid = Guid.NewGuid().ToString(),
        FilterUid = Guid.NewGuid().ToString(),

        Name = "the Name",
        FilterJson = "the Json",

        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filter);
      Assert.AreEqual(filter.CustomerUid, result.CustomerUID.ToString(),
        "customerUid has not been mapped correctly");
      Assert.AreEqual(filter.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filter.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filter.FilterUid, result.FilterUID.ToString(), "FilterUid has not been mapped correctly");
    }
  }
}
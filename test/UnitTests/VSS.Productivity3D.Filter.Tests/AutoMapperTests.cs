using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.Models;

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

        Name = "the name",
        FilterJson = "the Json",

        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter);
      Assert.AreEqual(filter.FilterUid, result.FilterUid, "filterUid has not been mapped correctly");
      Assert.AreEqual(filter.Name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filter.FilterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToCreateKafkaEvent_UserContext()
    {
      var filterRequest = FilterRequestFull.Create
      (
        customerUid: Guid.NewGuid().ToString(),
        isApplicationContext: false,
        userId: Guid.NewGuid().ToString(),
        projectUid: Guid.NewGuid().ToString(),
        filterUid: Guid.NewGuid().ToString(),
        name: "the name",
        filterJson: "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToCreateKafkaEvent_UserContext_NoFilterUID()
    {
      var filterRequest = FilterRequestFull.Create
      (
        customerUid: Guid.NewGuid().ToString(),
        isApplicationContext: false,
        userId: Guid.NewGuid().ToString(),
        projectUid: Guid.NewGuid().ToString(),
        filterUid: "",
        name: "the name",
        filterJson: "the Json"
      );
      filterRequest.filterUid = Guid.NewGuid().ToString();

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext()
    {
      var filterRequest = FilterRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        "ApplicationName",
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToCreateKafkaEvent_ApplicationContext_NoFilterUID()
    {
      var filterRequest = FilterRequestFull.Create
      (
        customerUid: Guid.NewGuid().ToString(),
        isApplicationContext: false,
        userId: "ApplicationName",
        projectUid: Guid.NewGuid().ToString(),
        filterUid: "",
        name: "the name",
        filterJson: "the Json"
      );
      filterRequest.filterUid = Guid.NewGuid().ToString();

      var result = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "name has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterJson, result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToUpdateKafkaEvent_OnlyNameUpdateable()
    {
      var filterRequest = FilterRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<UpdateFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.name, result.Name, "name has not been mapped correctly");
      Assert.IsNull(result.FilterJson, "ProjectType has not been mapped correctly");
    }

    [TestMethod]
    public void MapFilterRequestToDeleteKafkaEvent()
    {
      var filterRequest = FilterRequestFull.Create
      (
        Guid.NewGuid().ToString(),
        false,
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "the name",
        "the Json"
      );

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filterRequest);
      Assert.AreEqual(filterRequest.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filterRequest.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filterRequest.filterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
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

        Name = "the name",
        FilterJson = "the Json",

        IsDeleted = false,
        LastActionedUtc = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filter);
      Assert.AreEqual(filter.CustomerUid, result.CustomerUID.ToString(),
        "CustomerUid has not been mapped correctly");
      Assert.AreEqual(filter.UserId, result.UserID, "UserId has not been mapped correctly");
      Assert.AreEqual(filter.ProjectUid, result.ProjectUID.ToString(),
        "ProjectUid has not been mapped correctly");
      Assert.AreEqual(filter.FilterUid, result.FilterUID.ToString(), "filterUid has not been mapped correctly");
    }
  }
}
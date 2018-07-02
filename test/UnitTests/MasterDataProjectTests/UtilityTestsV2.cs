using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class UtilityTestsV2
  {
    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;
    private static string _customerUid;
    //private static byte[] _coordinateSystemFileContent;

  [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _boundaryLL = new List<TBCPoint>()
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };

      _checkBoundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";

      _businessCenterFile = new BusinessCenterFile()
      {
        FileSpaceId = "u710e3466-1d47-45e3-87b8-81d1127ed4ed",
        Path = "/BC Data/Sites/Chch Test Site",
        Name = "CTCTSITECAL.dc",
        CreatedUtc = DateTime.UtcNow.AddDays(-0.5)
      };

      _customerUid = Guid.NewGuid().ToString();
      //_coordinateSystemFileContent = new byte[] {0, 1, 2, 3, 4};
    }
    
    [TestMethod]
    public void MapCreateProjectV2RequestToEvent()
    {
      var requestedProjectType = ProjectType.ProjectMonitoring;
      var expectedProjectType = ProjectType.Standard;
      var request = CreateProjectV2Request.CreateACreateProjectV2Request
        (requestedProjectType, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var kafkaEvent = MapV2Models.MapCreateProjectV2RequestToEvent(request, _customerUid);

      Assert.IsTrue(Guid.TryParse(kafkaEvent.ProjectUID.ToString(), out var _), "ProjectUID has not been mapped correctly");
      Guid.TryParse(kafkaEvent.CustomerUID.ToString(), out var customerUidOut);
      Assert.AreEqual(_customerUid, customerUidOut.ToString(), "CustomerUID has not been mapped correctly");
      Assert.AreEqual(0, kafkaEvent.CustomerID, "CustomerID has not been mapped correctly");
      Assert.AreEqual(expectedProjectType, kafkaEvent.ProjectType, "ProjectType has not been mapped correctly");
      Assert.AreEqual(request.ProjectName, kafkaEvent.ProjectName, "ProjectName has not been mapped correctly");
      Assert.IsNull(kafkaEvent.Description, "Description has not been mapped correctly");
      Assert.AreEqual(request.ProjectStartDate, kafkaEvent.ProjectStartDate, "ProjectStartDate has not been mapped correctly");
      Assert.AreEqual(request.ProjectEndDate, kafkaEvent.ProjectEndDate, "ProjectEndDate has not been mapped correctly");
      Assert.AreEqual(request.ProjectTimezone, kafkaEvent.ProjectTimezone, "ProjectTimezone has not been mapped correctly");
      Assert.AreEqual(_checkBoundaryString, kafkaEvent.ProjectBoundary, "ProjectBoundary has not been mapped correctly");
      Assert.AreEqual(_businessCenterFile.Name, kafkaEvent.CoordinateSystemFileName, "CoordinateSystemFileName has not been mapped correctly");
      Assert.IsTrue(kafkaEvent.ActionUTC > DateTime.MinValue, "ActionUTC has not been mapped correctly");
      Assert.IsTrue(kafkaEvent.ReceivedUTC > DateTime.MinValue, "ReceivedUTC has not been mapped correctly");
    }
    
    [TestMethod]
    public void MapProjectToV4Result()
    {
      var project = new Repositories.DBModels.Project
      {
        ProjectUID = Guid.NewGuid().ToString(),
        LegacyProjectID = 123,
        ProjectType = ProjectType.ProjectMonitoring,
        Name = "the Name",
        Description = "the Description",
        ProjectTimeZone = "NZ stuff",
        LandfillTimeZone = "Pacific stuff",
        StartDate = new DateTime(2017, 01, 20),
        EndDate = new DateTime(2017, 02, 15),
        CustomerUID = Guid.NewGuid().ToString(),
        LegacyCustomerID = 0,

        SubscriptionUID = Guid.NewGuid().ToString(),
        SubscriptionStartDate = new DateTime(2017, 01, 20),
        SubscriptionEndDate = new DateTime(9999, 12, 31),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,

        GeometryWKT = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        CoordinateSystemFileName = "",
        CoordinateSystemLastActionedUTC = new DateTime(2017, 01, 21),

        IsDeleted = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(project);
      Assert.AreEqual(project.ProjectUID, result.ProjectUid, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(project.LegacyProjectID, result.LegacyProjectId, "LegacyProjectID has not been mapped correctly");
      Assert.AreEqual(project.ProjectType, result.ProjectType, "ProjectType has not been mapped correctly");
      Assert.AreEqual(project.Name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(project.Description, result.Description, "Description has not been mapped correctly");
      Assert.AreEqual(project.ProjectTimeZone, result.ProjectTimeZone, "ProjectTimeZone has not been mapped correctly");
      Assert.AreEqual(project.LandfillTimeZone, result.IanaTimeZone, "LandfillTimeZone has not been mapped correctly");
      Assert.AreEqual(project.StartDate.ToString("O"), result.StartDate, "StartDate has not been mapped correctly");
      Assert.AreEqual(project.EndDate.ToString("O"), result.EndDate, "EndDate has not been mapped correctly");
      Assert.AreEqual(project.CustomerUID, result.CustomerUid, "CustomerUID has not been mapped correctly");
      Assert.AreEqual(project.LegacyCustomerID.ToString(), result.LegacyCustomerId, "LegacyCustomerID has not been mapped correctly");
      Assert.AreEqual(project.SubscriptionUID, result.SubscriptionUid, "SubscriptionUID has not been mapped correctly");
      if (project.SubscriptionStartDate != null)
        Assert.AreEqual((object)project.SubscriptionStartDate.Value.ToString("O"), result.SubscriptionStartDate,
          "SubscriptionStartDate has not been mapped correctly");
      if (project.SubscriptionEndDate != null)
        Assert.AreEqual((object)project.SubscriptionEndDate.Value.ToString("O"), result.SubscriptionEndDate,
          "SubscriptionEndDate has not been mapped correctly");
      Assert.AreEqual(project.ServiceTypeID, (int)result.ServiceType, "ServiceTypeID has not been mapped correctly");
      Assert.AreEqual(project.GeometryWKT, result.ProjectGeofenceWKT, "GeometryWKT has not been mapped correctly");
      Assert.IsFalse(result.IsArchived, "IsArchived has not been mapped correctly");

      // just make a copy
      var copyOfProject = AutoMapperUtility.Automapper.Map<Repositories.DBModels.Project>(project);
      Assert.AreEqual(project.ProjectUID, copyOfProject.ProjectUID, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(project.LegacyProjectID, copyOfProject.LegacyProjectID, "LegacyProjectID has not been mapped correctly");
    }

    [TestMethod]
    public void MapProjectToV2Result()
    {
      var project = new Repositories.DBModels.Project
      {
        ProjectUID = Guid.NewGuid().ToString(),
        LegacyProjectID = 123,
        ProjectType = ProjectType.ProjectMonitoring,
        Name = "the Name",
        Description = "the Description",
        ProjectTimeZone = "NZ stuff",
        LandfillTimeZone = "Pacific stuff",
        StartDate = new DateTime(2017, 01, 20),
        EndDate = new DateTime(2017, 02, 15),
        CustomerUID = Guid.NewGuid().ToString(),
        LegacyCustomerID = 0,

        SubscriptionUID = Guid.NewGuid().ToString(),
        SubscriptionStartDate = new DateTime(2017, 01, 20),
        SubscriptionEndDate = new DateTime(9999, 12, 31),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,

        GeometryWKT = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        CoordinateSystemFileName = "",
        CoordinateSystemLastActionedUTC = new DateTime(2017, 01, 21),

        IsDeleted = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<ProjectV2DescriptorResult>(project);
      Assert.AreEqual(project.LegacyProjectID, result.LegacyProjectId, "LegacyProjectID has not been mapped correctly");
      Assert.AreEqual(project.Name, result.Name, "Name has not been mapped correctly");
      Assert.AreEqual(project.StartDate.ToString("O"), result.StartDate, "StartDate has not been mapped correctly");
      Assert.AreEqual(project.EndDate.ToString("O"), result.EndDate, "EndDate has not been mapped correctly");
      Assert.AreEqual(project.ProjectType, result.ProjectType, "ProjectType has not been mapped correctly");
    }
    
  }
}
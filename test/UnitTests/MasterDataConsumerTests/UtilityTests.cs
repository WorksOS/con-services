using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.Utilities;
using Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Visionlink.Project.UnitTests
{
  [TestClass]
  public class UtilityTests
  {
    [TestMethod]
    public void MapCreateProjectRequestToEvent()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        123, ProjectType.Standard, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        456, null, null);

      var kafkaEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      Assert.AreEqual(request.ProjectUID, kafkaEvent.ProjectUID, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.CustomerID, kafkaEvent.CustomerID, "CustomerID has not been mapped correctly");
      Assert.AreEqual(request.ProjectType, kafkaEvent.ProjectType, "ProjectType has not been mapped correctly");
      Assert.AreEqual(request.ProjectName, kafkaEvent.ProjectName, "ProjectName has not been mapped correctly");
      Assert.AreEqual(request.Description, kafkaEvent.Description, "Description has not been mapped correctly");
      Assert.AreEqual(request.ProjectStartDate, kafkaEvent.ProjectStartDate, "ProjectStartDate has not been mapped correctly");
      Assert.AreEqual(request.ProjectEndDate, kafkaEvent.ProjectEndDate, "ProjectEndDate has not been mapped correctly");
      Assert.AreEqual(request.ProjectTimezone, kafkaEvent.ProjectTimezone, "ProjectTimezone has not been mapped correctly");
      Assert.AreEqual(request.ProjectBoundary, kafkaEvent.ProjectBoundary, "ProjectBoundary has not been mapped correctly");
      Assert.AreEqual(request.ProjectID, kafkaEvent.ProjectID, "ProjectID has not been mapped correctly");
      Assert.AreEqual(request.CoordinateSystemFileName, kafkaEvent.CoordinateSystemFileName, "CoordinateSystemFileName has not been mapped correctly");
      Assert.AreEqual(request.CoordinateSystemFileContent, kafkaEvent.CoordinateSystemFileContent, "CoordinateSystemFileContent has not been mapped correctly");

      Assert.AreEqual(DateTime.MinValue, kafkaEvent.ActionUTC, "ActionUTC has not been mapped correctly");
      Assert.AreEqual(DateTime.MinValue, kafkaEvent.ReceivedUTC, "ReceivedUTC has not been mapped correctly");

      // just make a copy
      var copyOfRequest = AutoMapperUtility.Automapper.Map<CreateProjectRequest>(request);
      Assert.AreEqual(request.ProjectUID, copyOfRequest.ProjectUID, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.CoordinateSystemFileName, copyOfRequest.CoordinateSystemFileName, "CoordinateSystemFileName has not been mapped correctly");
    }

    [TestMethod]
    public void MapUpdateProjectRequestToEvent()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, "projectName", "this is the description",
        new DateTime(2017, 02, 15), "csName", new byte[] {1,2,3});

      var kafkaEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      Assert.AreEqual(request.ProjectUid, kafkaEvent.ProjectUID, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.ProjectType, kafkaEvent.ProjectType, "ProjectType has not been mapped correctly");
      Assert.AreEqual(request.ProjectName, kafkaEvent.ProjectName, "ProjectName has not been mapped correctly");
      Assert.AreEqual(request.Description, kafkaEvent.Description, "Description has not been mapped correctly");
      Assert.AreEqual(request.ProjectEndDate, kafkaEvent.ProjectEndDate, "ProjectEndDate has not been mapped correctly");
      Assert.AreEqual(request.CoordinateSystemFileName, kafkaEvent.CoordinateSystemFileName, "CoordinateSystemFileName has not been mapped correctly");
      CollectionAssert.AreEqual(request.CoordinateSystemFileContent, kafkaEvent.CoordinateSystemFileContent, "CoordinateSystemFileContent has not been mapped correctly");

      Assert.AreEqual(DateTime.MinValue, kafkaEvent.ActionUTC, "ActionUTC has not been mapped correctly");
      Assert.AreEqual(DateTime.MinValue, kafkaEvent.ReceivedUTC, "ReceivedUTC has not been mapped correctly");

      // just make a copy
      var copyOfRequest = AutoMapperUtility.Automapper.Map<UpdateProjectRequest>(request);
      Assert.AreEqual(request.ProjectUid, copyOfRequest.ProjectUid, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.CoordinateSystemFileName, copyOfRequest.CoordinateSystemFileName, "CoordinateSystemFileName has not been mapped correctly");
    }


    [TestMethod]
    public void MapProjectToResult()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var project = new Repositories.DBModels.Project()
      {
        ProjectUID = Guid.NewGuid().ToString(),
        LegacyProjectID = 123,
        ProjectType = ProjectType.ProjectMonitoring,
        Name = "the Name",
        Description = "the Description",
        ProjectTimeZone = "NZ stuff",
        LandfillTimeZone = "",
        StartDate = new DateTime(2017, 01, 20),
        EndDate = new DateTime(2017, 02, 15),
        CustomerUID = Guid.NewGuid().ToString(),
        LegacyCustomerID = 0,

        SubscriptionUID = Guid.NewGuid().ToString(),
        SubscriptionStartDate = new DateTime(2017, 01, 20),
        SubscriptionEndDate = new DateTime(9999, 12, 31),
        ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring,

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
      Assert.AreEqual(project.StartDate.ToString("O"), result.StartDate, "StartDate has not been mapped correctly");
      Assert.AreEqual(project.EndDate.ToString("O"), result.EndDate, "EndDate has not been mapped correctly");
      Assert.AreEqual(project.CustomerUID, result.CustomerUid, "CustomerUID has not been mapped correctly");
      Assert.AreEqual(project.LegacyCustomerID.ToString(), result.LegacyCustomerId, "LegacyCustomerID has not been mapped correctly");
      Assert.AreEqual(project.SubscriptionUID, result.SubscriptionUid, "SubscriptionUID has not been mapped correctly");
      Assert.AreEqual(project.SubscriptionStartDate.Value.ToString("O"), result.SubscriptionStartDate, "SubscriptionStartDate has not been mapped correctly");
      Assert.AreEqual(project.SubscriptionEndDate.Value.ToString("O"), result.SubscriptionEndDate, "SubscriptionEndDate has not been mapped correctly");
      Assert.AreEqual(project.ServiceTypeID, (int)result.ServiceType, "ServiceTypeID has not been mapped correctly");
      Assert.AreEqual(project.GeometryWKT, result.ProjectGeofenceWKT, "GeometryWKT has not been mapped correctly");
      Assert.IsFalse(result.IsArchived, "IsArchived has not been mapped correctly");

      // just make a copy
      var copyOfProject = AutoMapperUtility.Automapper.Map<Repositories.DBModels.Project>(project);
      Assert.AreEqual(project.ProjectUID, copyOfProject.ProjectUID, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(project.LegacyProjectID, copyOfProject.LegacyProjectID, "LegacyProjectID has not been mapped correctly");
    }

    //[TestMethod]
    //public void CanCreateFileAccessService()
    //{
    //  public IServiceProvider serviceProvider = null;

    //  [TestInitialize]
    //  public virtual void InitTest()
    //  {
    //    var serviceCollection = new ServiceCollection();

    //    string loggerRepoName = "UnitTestLogTest";
    //    var logPath = System.IO.Directory.GetCurrentDirectory();
    //    Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

    //    ILoggerFactory loggerFactory = new LoggerFactory();
    //    loggerFactory.AddDebug();
    //    loggerFactory.AddLog4Net(loggerRepoName);

    //    serviceCollection.AddLogging();
    //    serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
    //    serviceCollection.AddSingleton<IConfigurationStore, VSS.GenericConfiguration.GenericConfiguration>();
    //    serviceCollection.AddSingleton<IFileRepository, FileRepository>();
    //    serviceProvider = serviceCollection.BuildServiceProvider();

    //    var fileAccess = serviceProvider.GetRequiredService<IFileRepository>();
    //    Assert.IsNotNull(fileAccess);
    //}
  }
}

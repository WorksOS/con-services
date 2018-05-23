using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Authentication.JWT;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class UtilityTestsV3
  {
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void TestJWTKey()
    {
      const string jwt = "eyJhbGciOiJSUzI1NiIsIng1dCI6IlltRTNNelE0TVRZNE5EVTJaRFptT0RkbU5UUm1OMlpsWVRrd01XRXpZbU5qTVRrek1ERXpaZyJ9.eyJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9hcHBsaWNhdGlvbm5hbWUiOiJDb21wYWN0aW9uLURldmVsb3AtQ0kiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9wYXNzd29yZFBvbGljeURldGFpbHMiOiJleUoxY0dSaGRHVmtWR2x0WlNJNk1UUTVNVEUzTURFNE5qazNNaXdpYUdsemRHOXllU0k2V3lJMk5UTmlaakl5T0RnMk5qYzVOV1V3TkRFNU1qQTJOekUwWTJVek1EWmxNRE15WW1ReU1qWmlaRFUwWmpRek5qZzFOREkwTlRkbFpUSXhNRGcxTlRBd0lpd2lNakUyTnpkbU56bGlOVFZtWmpjek5qbGxNV1ZtT0RCaE5XRXdZVEZpWldJNE1qZzBaR0kwTXpZNU16QTNPVGt4WlRsalpEVTNORGcyTXpWallUZGxNaUlzSW1NNU5UQXdNRFpqTlRJelpXSTFPRGRoWkdFek1EVTFNakkwWVdSbFptRTNOMkl4TURjMllXUmxPVGcyTWpFMFpqSmpPREl6TWpZNE1HWXlOemsyTURVaVhYMD0iLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2VtYWlsVmVyaWZpZWQiOiJ0cnVlIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvc3Vic2NyaWJlciI6ImRldi12c3NhZG1pbkB0cmltYmxlLmNvbSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL3JvbGUiOiJwdWJsaXNoZXIiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9sYXN0VXBkYXRlVGltZVN0YW1wIjoiMTQ5NzI3ODIwNDkyMiIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvaWRlbnRpdHlcL3VubG9ja1RpbWUiOiIwIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2ZpcnN0bmFtZSI6IkRhdmUiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9wYXNzd29yZFBvbGljeSI6IkhJR0giLCJpc3MiOiJ3c28yLm9yZ1wvcHJvZHVjdHNcL2FtIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvbGFzdG5hbWUiOiJHbGFzc2VuYnVyeSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2FwcGxpY2F0aW9uaWQiOiIzNzQzIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvdmVyc2lvbiI6IjEuNCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2VuZHVzZXIiOiJkYXZpZF9nbGFzc2VuYnVyeUB0cmltYmxlLmNvbSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL3V1aWQiOiJjZTc5YjRiNy0yYTZmLTQ3NTUtOWNhOS0zZTQ5Yzg3ZWI0YjciLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE3NjM2MiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvaWRlbnRpdHlcL2ZhaWxlZExvZ2luQXR0ZW1wdHMiOiIwIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvaWRlbnRpdHlcL2FjY291bnRMb2NrZWQiOiJmYWxzZSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2FwaWNvbnRleHQiOiJcL3RcL3RyaW1ibGUuY29tXC92c3MtZGV2LXByb2plY3RzIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9zdGF0dXMiOiJleUpDVEU5RFMwVkVJam9pWm1Gc2MyVWlMQ0pYUVVsVVNVNUhYMFpQVWw5RlRVRkpURjlXUlZKSlJrbERRVlJKVDA0aU9pSm1ZV3h6WlNJc0lrSlNWVlJGWDBaUFVrTkZYMHhQUTB0RlJDSTZJbVpoYkhObElpd2lRVU5VU1ZaRklqb2lkSEoxWlNKOSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvYXBwbGljYXRpb250aWVyIjoiVW5saW1pdGVkIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvZW1haWxhZGRyZXNzIjoiRGF2aWRfR2xhc3NlbmJ1cnlAVHJpbWJsZS5jb20ifQ.d2n4ioMqEVmkQVYRcHaAhfayA1tt6b_Py6TlnFJtS2gL_b-gyU2g9g00sz1xq4gywPPZENhM1o6FX8dAA-HnVg2OIfp-unFDvB-jHo1-VEQxUQ--Ii04z0fE5Ed7NJkQjC-tUOpJD-wL62bACxB1e9nrpW8nlZoPACUUP6k6zI8";
      var jwtToken = new TPaaSJWT(jwt);
    }

    [TestMethod]
    public void MapCreateProjectRequestToEvent()
    {
      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        ProjectType.Standard, "projectName", "this is the description",
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
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, "projectName", "this is the description",
        new DateTime(2017, 02, 15), "csName", new byte[] { 1, 2, 3 });

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
    public void MapImportedFileRepoToResponse()
    {
      var request = new ImportedFile
      {
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
        Name = "this is the filename.svl",
        FileDescriptor = JsonConvert.SerializeObject(FileDescriptor.CreateFileDescriptor(Guid.NewGuid().ToString(), "/customerUID/projectUID", "this is the filename.svl")),
        FileCreatedUtc = DateTime.UtcNow.AddDays(-2),
        FileUpdatedUtc = DateTime.UtcNow.AddDays(-1),
        ImportedBy = "joeSmoe@trimble.com",
        SurveyedUtc = null,
        IsDeleted = false,
        LastActionedUtc = DateTime.UtcNow
      };

      var importedFileDescriptor = AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>(request);
      Assert.AreEqual(request.ProjectUid, importedFileDescriptor.ProjectUid, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.ImportedFileUid, importedFileDescriptor.ImportedFileUid, "ImportedFileUid has not been mapped correctly");
      Assert.AreEqual(request.CustomerUid, importedFileDescriptor.CustomerUid, "CustomerUid has not been mapped correctly");
      Assert.AreEqual(request.ImportedFileType, importedFileDescriptor.ImportedFileType, "ImportedFileType has not been mapped correctly");
      Assert.AreEqual(request.Name, importedFileDescriptor.Name, "Name has not been mapped correctly");
      Assert.AreEqual(request.FileCreatedUtc, importedFileDescriptor.FileCreatedUtc, "FileCreatedUtc has not been mapped correctly");
      Assert.AreEqual(request.FileUpdatedUtc, importedFileDescriptor.FileUpdatedUtc, "FileUpdatedUtc has not been mapped correctly");
      Assert.AreEqual(request.ImportedBy, importedFileDescriptor.ImportedBy, "ImportedBy has not been mapped correctly");
      Assert.AreEqual(request.SurveyedUtc, importedFileDescriptor.SurveyedUtc, "SurveyedUtc has not been mapped correctly");
      Assert.AreEqual(request.LastActionedUtc, importedFileDescriptor.ImportedUtc, "ImportedUtc has not been mapped correctly");
      Assert.AreEqual(true, importedFileDescriptor.IsActivated, "IsActivated has not been mapped correctly");

      // just make a copy file descriptor is only in the source file, not the destination
      var copyOfRequest = AutoMapperUtility.Automapper.Map<ImportedFile>(request);
      Assert.AreEqual(request.ProjectUid, copyOfRequest.ProjectUid, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.FileDescriptor, copyOfRequest.FileDescriptor, "FileDescriptor has not been mapped correctly");
    }

    [TestMethod]
    public void MapImportedFileRepoToUpdateEvent()
    {
      var request = new ImportedFile
      {
        ProjectUid = Guid.NewGuid().ToString(),
        ImportedFileUid = Guid.NewGuid().ToString(),
        CustomerUid = Guid.NewGuid().ToString(),
        ImportedFileType = ImportedFileType.Alignment,
        Name = "this is the filename.svl",
        FileDescriptor = JsonConvert.SerializeObject(FileDescriptor.CreateFileDescriptor(Guid.NewGuid().ToString(), "/customerUID/projectUID", "this is the filename.svl")),
        FileCreatedUtc = DateTime.UtcNow.AddDays(-2),
        FileUpdatedUtc = DateTime.UtcNow.AddDays(-1),
        ImportedBy = "joeSmoe@trimble.com",
        SurveyedUtc = null,
        IsDeleted = false,
        LastActionedUtc = DateTime.UtcNow
      };

      var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(request);
      Assert.AreEqual(request.LastActionedUtc, updateImportedFileEvent.ActionUTC, "ActionUTC has not been mapped correctly");
      Assert.AreEqual(request.FileCreatedUtc, updateImportedFileEvent.FileCreatedUtc, "FileCreatedUtc has not been mapped correctly");
      Assert.AreEqual(request.FileDescriptor, updateImportedFileEvent.FileDescriptor, "FileDescriptor has not been mapped correctly");
      Assert.AreEqual(request.FileUpdatedUtc, updateImportedFileEvent.FileUpdatedUtc, "FileUpdatedUtc has not been mapped correctly");
      Assert.AreEqual(request.ImportedBy, updateImportedFileEvent.ImportedBy, "ImportedBy has not been mapped correctly");
      Assert.AreEqual(request.ImportedFileUid, updateImportedFileEvent.ImportedFileUID.ToString(), "ImportedFileUID has not been mapped correctly");
      Assert.AreEqual(request.ProjectUid, updateImportedFileEvent.ProjectUID.ToString(), "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.LastActionedUtc, updateImportedFileEvent.ReceivedUTC, "ReceivedUTC has not been mapped correctly");
      Assert.AreEqual(request.SurveyedUtc, updateImportedFileEvent.SurveyedUtc, "SurveyedUtc has not been mapped correctly");

      // just make a copy file descriptor is only in the source file, not the destination
      var copyOfRequest = AutoMapperUtility.Automapper.Map<ImportedFile>(request);
      Assert.AreEqual(request.ProjectUid, copyOfRequest.ProjectUid, "ProjectUID has not been mapped correctly");
      Assert.AreEqual(request.FileDescriptor, copyOfRequest.FileDescriptor, "FileDescriptor has not been mapped correctly");
    }

    [TestMethod]
    public void CanCalculateProjectBoundaryArea()
    {
      var geometryWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))";
      var area = ProjectBoundaryValidator.CalculateAreaSqMeters(geometryWKT);
      Assert.AreEqual(375300.594251673, area, 0.00001);
    }
  }
}
using System;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Extensions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests
{
  public class UtilityTestsV6
  {
    public UtilityTestsV6()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public void MapProjectToResult()
    {
      var project = new ProjectDatabaseModel
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Name = "the Name",
        ProjectTimeZone = "NZ stuff",
        ProjectTimeZoneIana = "Pacific stuff",
        CustomerUID = Guid.NewGuid().ToString(),
        Boundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        CoordinateSystemFileName = "",
        CoordinateSystemLastActionedUTC = new DateTime(2017, 01, 21),

        IsArchived = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project);
      Assert.Equal(project.ProjectUID, result.ProjectUid);
      Assert.Equal(new Guid(project.ProjectUID).ToLegacyId(), result.ShortRaptorProjectId);
      Assert.Equal(project.ProjectType, result.ProjectType);
      Assert.Equal(project.Name, result.Name);
      Assert.Equal(project.ProjectTimeZone, result.ProjectTimeZone);
      Assert.Equal(project.ProjectTimeZoneIana, result.IanaTimeZone);
      Assert.Equal(project.CustomerUID, result.CustomerUid);
      Assert.Equal(project.Boundary, result.ProjectGeofenceWKT);
      Assert.False(result.IsArchived, "IsArchived has not been mapped correctly");

      // just make a copy
      var copyOfProject = AutoMapperUtility.Automapper.Map<ProjectDatabaseModel>(project);
      Assert.Equal(project.ProjectUID, copyOfProject.ProjectUID);
      Assert.Equal(project.ShortRaptorProjectId, copyOfProject.ShortRaptorProjectId);
    }

    [Fact]
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
        ParentUid = null,
        Offset = 0,
        IsDeleted = false,
        LastActionedUtc = DateTime.UtcNow
      };

      var importedFileDescriptor = AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>(request);
      Assert.Equal(request.ProjectUid, importedFileDescriptor.ProjectUid);
      Assert.Equal(request.ImportedFileUid, importedFileDescriptor.ImportedFileUid);
      Assert.Equal(request.CustomerUid, importedFileDescriptor.CustomerUid);
      Assert.Equal(request.ImportedFileType, importedFileDescriptor.ImportedFileType);
      Assert.Equal(request.Name, importedFileDescriptor.Name);
      Assert.Equal(request.FileCreatedUtc, importedFileDescriptor.FileCreatedUtc);
      Assert.Equal(request.FileUpdatedUtc, importedFileDescriptor.FileUpdatedUtc);
      Assert.Equal(request.ImportedBy, importedFileDescriptor.ImportedBy);
      Assert.Equal(request.SurveyedUtc, importedFileDescriptor.SurveyedUtc);
      Assert.Equal(request.ParentUid, importedFileDescriptor.ParentUid.HasValue ? "Fail assertion" : null);
      Assert.Equal(request.Offset, importedFileDescriptor.Offset);
      Assert.Equal(request.LastActionedUtc, importedFileDescriptor.ImportedUtc);
      Assert.True(importedFileDescriptor.IsActivated);

      // just make a copy file descriptor is only in the source file, not the destination
      var copyOfRequest = AutoMapperUtility.Automapper.Map<ImportedFile>(request);
      Assert.Equal(request.ProjectUid, copyOfRequest.ProjectUid);
      Assert.Equal(request.FileDescriptor, copyOfRequest.FileDescriptor);
    }

    [Fact]
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
        ParentUid = null,
        Offset = 0,
        IsDeleted = false,
        LastActionedUtc = DateTime.UtcNow
      };

      var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(request);
      Assert.Equal(request.LastActionedUtc, updateImportedFileEvent.ActionUTC);
      Assert.Equal(request.FileCreatedUtc, updateImportedFileEvent.FileCreatedUtc);
      Assert.Equal(request.FileDescriptor, updateImportedFileEvent.FileDescriptor);
      Assert.Equal(request.FileUpdatedUtc, updateImportedFileEvent.FileUpdatedUtc);
      Assert.Equal(request.ImportedBy, updateImportedFileEvent.ImportedBy);
      Assert.Equal(request.ImportedFileUid, updateImportedFileEvent.ImportedFileUID.ToString());
      Assert.Equal(request.ProjectUid, updateImportedFileEvent.ProjectUID.ToString());
      Assert.Equal(request.SurveyedUtc, updateImportedFileEvent.SurveyedUtc);

      // just make a copy file descriptor is only in the source file, not the destination
      var copyOfRequest = AutoMapperUtility.Automapper.Map<ImportedFile>(request);
      Assert.Equal(request.ProjectUid, copyOfRequest.ProjectUid);
      Assert.Equal(request.FileDescriptor, copyOfRequest.FileDescriptor);
    }

    [Fact]
    public void CanCalculateProjectBoundaryArea()
    {
      var geometryWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))";
      var area = GeofenceValidation.CalculateAreaSqMeters(geometryWKT);
      Assert.Equal(375300.594251673, area, 5);
    }
  }
}

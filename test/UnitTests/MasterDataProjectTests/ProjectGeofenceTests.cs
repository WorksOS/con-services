using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectGeofenceTests : ExecutorBaseTests
  {
    protected ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();
    private readonly string _validBoundary;
    private List<GeofenceWithAssociation> _geofencesWithAssociation;
    private static string _customerUid;
    private static string _projectUid;

    public ProjectGeofenceTests()
    {
      _validBoundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      _geofencesWithAssociation = new List<GeofenceWithAssociation>()
      {
        new GeofenceWithAssociation()
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name",
          Description = "geofence Description",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 12.45
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name2",
          Description = "geofence Description2",
          GeofenceType = GeofenceType.Project,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 223.45
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name3",
          Description = "geofence Description3",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45,
          ProjectUID = _projectUid   // only this one is associated
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name4",
          Description = "geofence Description4",
          GeofenceType = GeofenceType.CutZone,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45
        }
      };
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _customerUid = Guid.NewGuid().ToString();
      _projectUid = Guid.NewGuid().ToString();
    }

    [TestMethod]
    public void ValidateCopyGeofenceResult()
    {
      var result = new GeofenceV4DescriptorsListResult
      {
        GeofenceDescriptors = _geofencesWithAssociation.Select(geofence =>
            AutoMapperUtility.Automapper.Map<GeofenceV4Descriptor>(geofence))
          .ToImmutableList()
      };

      Assert.AreEqual(4, result.GeofenceDescriptors.Count, "Should be 4 geofences");
      Assert.AreEqual(10, (int) result.GeofenceDescriptors[0].GeofenceType, "Should be Landfill type");
      
      Assert.AreEqual(_geofencesWithAssociation[1].GeofenceUID, result.GeofenceDescriptors[1].GeofenceUid, "Unexpected GeofenceUid");
      Assert.AreEqual(_geofencesWithAssociation[1].Name, result.GeofenceDescriptors[1].Name, "Unexpected project name");
      Assert.AreEqual(1, (int)result.GeofenceDescriptors[1].GeofenceType, "Should be Project type");
      Assert.AreEqual(_geofencesWithAssociation[1].GeometryWKT, result.GeofenceDescriptors[1].GeometryWKT, "Unexpected GeometryWKT");
      Assert.AreEqual(_geofencesWithAssociation[1].FillColor, result.GeofenceDescriptors[1].FillColor, "Unexpected FillColor");
      Assert.AreEqual(_geofencesWithAssociation[1].IsTransparent, result.GeofenceDescriptors[1].IsTransparent, "Unexpected IsTransparent");
      Assert.AreEqual(_geofencesWithAssociation[1].Description, result.GeofenceDescriptors[1].Description, "Unexpected Description");
      Assert.AreEqual(_geofencesWithAssociation[1].CustomerUID, result.GeofenceDescriptors[1].CustomerUid, "Unexpected CustomerUid");
      Assert.AreEqual(_geofencesWithAssociation[1].UserUID, result.GeofenceDescriptors[1].UserUid, "Unexpected UserUid");
      Assert.AreEqual(_geofencesWithAssociation[1].AreaSqMeters, result.GeofenceDescriptors[1].AreaSqMeters, "Unexpected AreaSqMeters");
    }

    [TestMethod]
    public async Task Get_UnassignedLandfillGeofencesAsync()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectGeofenceTests>();

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(_geofencesWithAssociation);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      var geofences = await ProjectRequestHelper.GetGeofenceList(_customerUid, string.Empty, geofenceTypes, log, projectRepo.Object)
        .ConfigureAwait(false);

      Assert.AreEqual(1, geofences.Count, "Should be 1 landfill");
      
      Assert.AreEqual(_geofencesWithAssociation[0].GeofenceUID, geofences[0].GeofenceUID, "Unexpected GeofenceUid");
      Assert.AreEqual(_geofencesWithAssociation[0].Name, geofences[0].Name, "Unexpected project name");
      Assert.AreEqual(_geofencesWithAssociation[0].GeofenceType, geofences[0].GeofenceType, "Should be Landfill type");
      Assert.AreEqual(_geofencesWithAssociation[0].GeometryWKT, geofences[0].GeometryWKT, "Unexpected GeometryWKT");
      Assert.AreEqual(_geofencesWithAssociation[0].FillColor, geofences[0].FillColor, "Unexpected FillColor");
      Assert.AreEqual(_geofencesWithAssociation[0].IsTransparent, geofences[0].IsTransparent, "Unexpected IsTransparent");
      Assert.AreEqual(_geofencesWithAssociation[0].Description, geofences[0].Description, "Unexpected Description");
      Assert.AreEqual(_geofencesWithAssociation[0].CustomerUID, geofences[0].CustomerUID, "Unexpected CustomerUid");
      Assert.AreEqual(_geofencesWithAssociation[0].UserUID, geofences[0].UserUID, "Unexpected UserUid");
      Assert.AreEqual(_geofencesWithAssociation[0].AreaSqMeters, geofences[0].AreaSqMeters, "Unexpected AreaSqMeters");
    }

    [TestMethod]
    public async Task Get_AssignedLandfillGeofences_FromProject()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectGeofenceTests>();

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(_geofencesWithAssociation);

      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };

      var geofences = await ProjectRequestHelper.GetGeofenceList(_customerUid, _projectUid, geofenceTypes, log, projectRepo.Object)
        .ConfigureAwait(false);

      Assert.AreEqual(1, geofences.Count, "Should be 1 landfills");

      Assert.AreEqual(_geofencesWithAssociation[2].GeofenceUID, geofences[0].GeofenceUID, "Unexpected GeofenceUid");
      Assert.AreEqual(_geofencesWithAssociation[2].Name, geofences[0].Name, "Unexpected project name");
      Assert.AreEqual(_geofencesWithAssociation[2].GeofenceType, geofences[0].GeofenceType, "Should be Landfill type");
      Assert.AreEqual(_geofencesWithAssociation[2].GeometryWKT, geofences[0].GeometryWKT, "Unexpected GeometryWKT");
      Assert.AreEqual(_geofencesWithAssociation[2].FillColor, geofences[0].FillColor, "Unexpected FillColor");
      Assert.AreEqual(_geofencesWithAssociation[2].IsTransparent, geofences[0].IsTransparent, "Unexpected IsTransparent");
      Assert.AreEqual(_geofencesWithAssociation[2].Description, geofences[0].Description, "Unexpected Description");
      Assert.AreEqual(_geofencesWithAssociation[2].CustomerUID, geofences[0].CustomerUID, "Unexpected CustomerUid");
      Assert.AreEqual(_geofencesWithAssociation[2].UserUID, geofences[0].UserUID, "Unexpected UserUid");
      Assert.AreEqual(_geofencesWithAssociation[2].AreaSqMeters, geofences[0].AreaSqMeters, "Unexpected AreaSqMeters");
    }
  }
}
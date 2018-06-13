using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectGeofenceValidationTests : ExecutorBaseTests
  {
    protected ProjectErrorCodesProvider _projectErrorCodesProvider = new ProjectErrorCodesProvider();
    private readonly List<GeofenceWithAssociation> _geofencesWithAssociation;
    private static string _customerUid;
    private static string _projectUid;

    public ProjectGeofenceValidationTests()
    {
      var validBoundary =
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      _geofencesWithAssociation = new List<GeofenceWithAssociation>()
      {
        new GeofenceWithAssociation()
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name",
          Description = "geofence Description",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = validBoundary,
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
          GeometryWKT = validBoundary,
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
          GeometryWKT = validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45,
          ProjectUID = _projectUid // only this one is associated
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name4",
          Description = "geofence Description4",
          GeofenceType = GeofenceType.CutZone,
          GeometryWKT = validBoundary,
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

      Assert.AreEqual(_geofencesWithAssociation[1].GeofenceUID, result.GeofenceDescriptors[1].GeofenceUid,
        "Unexpected GeofenceUid");
      Assert.AreEqual(_geofencesWithAssociation[1].Name, result.GeofenceDescriptors[1].Name, "Unexpected project name");
      Assert.AreEqual(1, (int) result.GeofenceDescriptors[1].GeofenceType, "Should be Project type");
      Assert.AreEqual(_geofencesWithAssociation[1].GeometryWKT, result.GeofenceDescriptors[1].GeometryWKT,
        "Unexpected GeometryWKT");
      Assert.AreEqual(_geofencesWithAssociation[1].FillColor, result.GeofenceDescriptors[1].FillColor,
        "Unexpected FillColor");
      Assert.AreEqual(_geofencesWithAssociation[1].IsTransparent, result.GeofenceDescriptors[1].IsTransparent,
        "Unexpected IsTransparent");
      Assert.AreEqual(_geofencesWithAssociation[1].Description, result.GeofenceDescriptors[1].Description,
        "Unexpected Description");
      Assert.AreEqual(_geofencesWithAssociation[1].CustomerUID, result.GeofenceDescriptors[1].CustomerUid,
        "Unexpected CustomerUid");
      Assert.AreEqual(_geofencesWithAssociation[1].UserUID, result.GeofenceDescriptors[1].UserUid,
        "Unexpected UserUid");
      Assert.AreEqual(_geofencesWithAssociation[1].AreaSqMeters, result.GeofenceDescriptors[1].AreaSqMeters,
        "Unexpected AreaSqMeters");
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_HappyPath()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};
      var geofences = new List<Guid>() {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);
      request.Validate();
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_MissingProjectUid()
    {
      var projectUid = Guid.Empty;
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};
      var geofences = new List<Guid>() {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(5), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceTypes1()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>() { };
      var geofences = new List<Guid>() {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(73), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceTypes2()
    {
      var projectUid = Guid.NewGuid();
      var geofences = new List<Guid>() {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, null, geofences);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(73), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_UnsupportedGeofenceTypes()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.CutZone};
      var geofences = new List<Guid>() {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(102), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceUids1()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, null);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(103), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceUids2()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};
      var geofences = new List<Guid>() { };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(103), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceUids3()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};
      var geofences = new List<Guid>() {Guid.Empty};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(103), StringComparison.Ordinal));
    }
  }
}
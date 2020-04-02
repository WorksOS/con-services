using System;
using System.Collections.Generic;
using System.Linq;
using RepositoryTests.Internal;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests
{
  public class ProjectSpatialRepositoryTests : TestControllerBase
  {
    ProjectRepository projectRepo;

    public ProjectSpatialRepositoryTests()
    {
      SetupLogging();
      projectRepo = new ProjectRepository(configStore, loggerFactory);
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [Fact]
    public void PointInsideStandardProjectBoundary()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      projectRepo.StoreEvent(createProjectEvent).Wait();
     
      var g = projectRepo.GetIntersectingProjects(createProjectEvent.CustomerUID.ToString(), 15, 180, createProjectEvent.ProjectStartDate.AddDays(1)); g.Wait();
      var projects = g.Result;
      Assert.NotNull(g.Result);
      Assert.Single(g.Result);
      Assert.Equal(projects.ToList()[0].ProjectUID, createProjectEvent.ProjectUID.ToString());
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [Fact]
    public void PointOutsideStandardProjectBoundary()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
           
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();      

      var g = projectRepo.GetIntersectingProjects(createProjectEvent.CustomerUID.ToString(), 50, 180, createProjectEvent.ProjectStartDate.AddDays(1)); g.Wait();
      Assert.NotNull(g.Result);
      Assert.Empty(g.Result);
    }

    /// <summary>
    /// Point is within the projectboundary - Happy path i.e. 
    ///   customer, project and CustomerProject exit
    ///   project has boundary
    /// </summary>
    [Fact]
    public void PointOnPointStandardProjectBoundary()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();

      var g = projectRepo.GetIntersectingProjects(createProjectEvent.CustomerUID.ToString(), 40, 170, createProjectEvent.ProjectStartDate.AddDays(1)); g.Wait();
      var projects = g.Result;
      Assert.NotNull(g.Result);
      Assert.Single(g.Result);
      Assert.Equal(projects.ToList()[0].ProjectUID, createProjectEvent.ProjectUID.ToString());
    }

   
    /// <summary>
    /// Polygon is within (internal) an existing projectboundary
    ///    and time is within
    /// </summary>
    [Fact]
    public void PolygonIntersection_InternalBoundaryInternalTime()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();

      string testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var testCustomerUID = createProjectEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.True(g.Result);
    }

    /// <summary>
    /// Polygon is not within an existing projectboundary
    ///    but time is overlapping
    /// </summary>
    [Fact]
    public void PolygonIntersection_InternalBoundaryExternalTime()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      projectRepo.StoreEvent(createProjectEvent).Wait();

      string testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var testCustomerUID = createProjectEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectEndDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectEndDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.False(g.Result);
    }

    /// <summary>
    /// Polygon is not within an existing projectboundary
    ///    but time is within
    /// </summary>
    [Fact]
    public void PolygonIntersection_ExternalBoundaryInternalTime()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();

      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 200 20, 200 10))";
      var testCustomerUID = createProjectEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.False(g.Result);
    }

    /// <summary>
    /// Polygon is completely overlapping an existing projectboundary
    ///    and time is within
    /// </summary>
    [Fact]
    public void PolygonIntersection_OverlappingBoundaryInternalTime()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      projectRepo.StoreEvent(createProjectEvent).Wait();

      string testBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      var testCustomerUID = createProjectEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.True(g.Result);
    }

    /// <summary>
    /// Polygon touches at a point an existing projectboundary
    ///    and time is within
    /// </summary>
    [Fact]
    public void PolygonIntersection_TouchingBoundaryInternalTime()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();

      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 190 20, 200 10))";
      var testCustomerUID = createProjectEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.True(g.Result);
    }

    /// <summary>
    /// Polygon overlaps but no internal points, an existing projectboundary
    ///    and time is within
    /// </summary>
    [Fact]
    public void PolygonIntersection_OverlapExternalBoundaryInternalTime()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();

      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 175 45, 200 10))";
      var testCustomerUID = createProjectEvent.CustomerUID.ToString();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUID, testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.True(g.Result);
    }

    /// <summary>
    /// Polygon is within (internal) an existing projectboundary
    ///    and time is within
    /// </summary>
    [Fact]
    public void PolygonIntersection_InternalBoundaryInternalTimeDifferentCustomer()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };
      
      projectRepo.StoreEvent(createProjectEvent).Wait();

      var testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";
      var testCustomerUid = Guid.NewGuid();
      var testStartDate = createProjectEvent.ProjectStartDate.AddDays(1);
      var testEndDate = createProjectEvent.ProjectStartDate.AddDays(3);

      var g = projectRepo.DoesPolygonOverlap(testCustomerUid.ToString(), testBoundary, testStartDate, testEndDate); g.Wait();
      Assert.False(g.Result);
    }

    /// <summary>
    /// When updating a project, 
    ///     shouldn't see the this nominated project as overlapping
    /// </summary>
    [Fact]
    public void PolygonIntersection_UpdateProjectBoundary()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
      };

      var g = projectRepo.DoesPolygonOverlap(createProjectEvent.CustomerUID.ToString(), createProjectEvent.ProjectBoundary, createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate); g.Wait();
      Assert.False(g.Result);

      projectRepo.StoreEvent(createProjectEvent).Wait();
      g = projectRepo.DoesPolygonOverlap(createProjectEvent.CustomerUID.ToString(), createProjectEvent.ProjectBoundary, createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate, createProjectEvent.ProjectUID.ToString()); g.Wait();
      Assert.False(g.Result);

      g = projectRepo.DoesPolygonOverlap(createProjectEvent.CustomerUID.ToString(), createProjectEvent.ProjectBoundary, createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate); g.Wait();
      Assert.True(g.Result);
    }

    /// <summary>
    /// Polygon is within (internal) a project boundary
    /// </summary>
    [Fact]
    public void PolygonIntersection_InternalBoundary()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.True(g.Result.First());
    }

    /// <summary>
    /// Polygon is not within the project boundary
    /// </summary>
    [Fact]
    public void PolygonIntersection_ExternalBoundary()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 200 20, 200 10))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.False(g.Result.First());
    }

    /// <summary>
    /// Polygon is completely overlapping the project boundary
    /// </summary>
    [Fact]
    public void PolygonIntersection_OverlappingBoundary()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.True(g.Result.First());
    }

    /// <summary>
    /// Polygon touches at a point a project boundary
    /// </summary>
    [Fact]
    public void PolygonIntersection_TouchingBoundary()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 190 20, 200 10))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.True(g.Result.First());
    }

    /// <summary>
    /// Polygon overlaps but no internal points, the project boundary
    /// </summary>
    [Fact]
    public void PolygonIntersection_OverlapExternalBoundary()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "POLYGON((200 10, 202 10, 202 20, 175 45, 200 10))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.True(g.Result.First());
    }

    [Fact]
    public void PolygonIntersections()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";

      string testBoundary1 = "POLYGON((175 15, 185 15, 185 35, 175 35, 175 15))"; //Inside
      string testBoundary2 = "POLYGON((200 10, 202 10, 202 20, 200 20, 200 10))"; //Outside
      string testBoundary3 = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"; //Completely overlapping
      string testBoundary4 = "POLYGON((200 10, 202 10, 202 20, 190 20, 200 10))"; //Touches at a point
      string testBoundary5 = "POLYGON((200 10, 202 10, 202 20, 175 45, 200 10))"; //Overlaps but no internal points

      var testBoundaries = new List<string>
      {
        testBoundary1, testBoundary2, testBoundary3, testBoundary4, testBoundary5
      };
      var g = projectRepo.DoPolygonsOverlap(projectBoundary, testBoundaries); g.Wait();
      var results = g.Result.ToList();
      Assert.Equal(testBoundaries.Count(), results.Count);
      for (var i=0; i<results.Count; i++)
      {
        Assert.Equal(i != 1, results[i]);
      }
    }

    [Fact]
    public void PolygonIntersection_MissingGeofence()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, null); g.Wait();
      Assert.False(g.Result.Any());
    }

    [Fact]
    public void PolygonIntersection_NotPolygonGeofence()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "LINESTRING(30 10, 10 30, 40 40)";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.False(g.Result.First());
    }

    [Fact]
    public void PolygonIntersection_InvalidGeofence()
    {
      var projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))";
      string testBoundary = "POLYGON((1 3,3 2,1 1,3 0,1 0,1 3))";

      var g = projectRepo.DoPolygonsOverlap(projectBoundary, new List<string>() { testBoundary }); g.Wait();
      Assert.False(g.Result.First());
    }

  }
}

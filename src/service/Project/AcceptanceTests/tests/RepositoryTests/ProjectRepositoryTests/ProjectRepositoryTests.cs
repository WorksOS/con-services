using System;
using System.Linq;
using RepositoryTests.Internal;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests.ProjectRepositoryTests
{
  public class ProjectRepositoryTests : TestControllerBase
  {
    ProjectRepository projectRepo;
    GeofenceRepository geofenceRepo;

    public ProjectRepositoryTests()
    {
      SetupLogging();
      projectRepo = new ProjectRepository(configStore, loggerFactory);
      geofenceRepo = new GeofenceRepository(configStore, loggerFactory);
    }


    #region Projects


    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   project doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateProject_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);      

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        Description = "the Description",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs"
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result, 
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType, 
        createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
        createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, createProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);
           
      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);

      var pg = projectRepo.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      pg.Wait();
      Assert.NotNull(pg.Result);
      var projectGeofenceList = pg.Result.ToList();
      Assert.Equal(1, projectGeofenceList.Count);
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   project doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateProject_HappyPath_Unicode()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
            
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = new Random().Next(1, 1999999),
        ProjectName = "The Project Name(株)城内組　二見地区築堤護岸工事",
        Description = "the Description(株)城内組　二見地区築堤護岸工事",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs"
      };

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
       createProjectEvent.ProjectUID.ToString().ToString(), createProjectEvent.CustomerUID.ToString(), 0,
       createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
       createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
       createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
       createProjectEvent.ProjectBoundary, createProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   project doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateProjectWithCustomer_HappyPath_NoRaptorProjectId()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = 0,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.Standard,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
         createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
         createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, createProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);
      
      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
     ///   project doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateProjectWithCustomer_HappyPath_DuplicateLegacyShortRaptorProjectId()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent1 = new CreateProjectEvent
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };

      var createProjectEvent2 = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ShortRaptorProjectId = createProjectEvent1.ShortRaptorProjectId,
        ProjectName = "The Project Name 2",
        ProjectType = createProjectEvent1.ProjectType,
        ProjectTimezone = createProjectEvent1.ProjectTimezone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };


      var s1 = projectRepo.StoreEvent(createProjectEvent1);
      s1.Wait();
      Assert.Equal(1, s1.Result);

      var s2 = projectRepo.StoreEvent(createProjectEvent2);
      s2.Wait();
      Assert.Equal(0, s2.Result);

      CheckProjectHistoryCount(createProjectEvent1.ProjectUID.ToString(), 1);
      CheckProjectHistoryCount(createProjectEvent2.ProjectUID.ToString(), 0);
    }

    /// <summary>
    /// Create Project - Project already exists
    ///   project exists but is different.
    /// </summary>
    [Fact]
    public void CreateProjectWithCustomer_ProjectExists()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
           

      createProjectEvent.ActionUTC = createProjectEvent.ActionUTC.AddMinutes(-2);
      projectRepo.StoreEvent(createProjectEvent).Wait();

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
        createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
        createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, createProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Update Project - Happy path i.e. 
    ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [Fact]
    public void UpdateProject_HappyPath()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",

        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs",
        ActionUTC = actionUtc
      };
      
      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(1)
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
      s = projectRepo.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);     

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      CompareProject(g.Result,
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        updateProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
        createProjectEvent.ProjectStartDate, updateProjectEvent.ProjectEndDate,
        createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, updateProjectEvent.ActionUTC, updateProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Null sent in Update event to be ignored
    /// </summary>
    [Fact]
    public void UpdateProject_IgnoreNulls()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectType = createProjectEvent.ProjectType,
        ProjectEndDate = createProjectEvent.ProjectEndDate,
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
        createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
        createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, updateProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Update Project - earlier ActionUTC 
    ///   project exists and New ActionUTC is earlier than its LastActionUTC.
    /// </summary>
    [Fact]
    public void UpdateProject_OldUpdate()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };
      
      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = actionUtc.AddHours(-1)
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);


      s = projectRepo.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.Equal(0, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
             createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
             createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
             createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
             createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
             createProjectEvent.ProjectBoundary, createProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Update Project - then the Create is applied
    /// </summary>
    [Fact]
    public void UpdateProject_ThenCreateEvent()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };
           
      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = actionUtc.AddHours(1)
      };

      
      var s = projectRepo.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
      
      s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
           
      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      CompareProject(g.Result,
         createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
         updateProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
         createProjectEvent.ProjectStartDate, updateProjectEvent.ProjectEndDate,
         createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
         createProjectEvent.ProjectBoundary, updateProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Update Project - then the Create is applied
    /// </summary>
    [Fact]
    public void UpdateProject_ThenCreateEvent_WhereUpdateMissingTimeZone()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };
      
      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
       
        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = projectRepo.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        updateProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
        createProjectEvent.ProjectStartDate, updateProjectEvent.ProjectEndDate,
        createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, updateProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);
    }

    /// <summary>
    /// Update Project - Update projectBoundary
    ///    Updates to new boundary.
    ///    Then update includes empty String - i.e. don't update. 
    /// </summary>
    [Fact]
    public void UpdateProject_HappyPath_UpdatesBoundary()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      string originalProjectBoundary =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      string updatedProjectBoundary =
        "POLYGON((-121 38,-121 38,-121 38,-121 38,-121 38,-121 38))";
     
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
        ProjectBoundary = originalProjectBoundary,
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs",
        ActionUTC = actionUtc
      };
      
      var firstUpdateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The FirstUpdated Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = updatedProjectBoundary,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(1)
      };

      var secondUpdateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The SecondUpdated Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = string.Empty,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(2)
      };

      var thirdUpdateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The ThirdUpdated Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = null,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(3)
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
          
      s = projectRepo.StoreEvent(firstUpdateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
       createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
       firstUpdateProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
       createProjectEvent.ProjectStartDate, firstUpdateProjectEvent.ProjectEndDate,
       createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
       firstUpdateProjectEvent.ProjectBoundary, firstUpdateProjectEvent.ActionUTC, firstUpdateProjectEvent.CoordinateSystemFileName);
      
      s = projectRepo.StoreEvent(secondUpdateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(firstUpdateProjectEvent.ProjectBoundary, g.Result.Boundary);

      s = projectRepo.StoreEvent(thirdUpdateProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(firstUpdateProjectEvent.ProjectBoundary, g.Result.Boundary);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 4);
    }


    /// <summary>
    /// Delete Project - then the Create is applied
    /// </summary>
    [Fact]
    public void DeleteProject_ThenCreateEvent()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = projectRepo.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);
            
      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Delete Project - Happy path
    /// </summary>
    [Fact]
    public void DeleteProject_HappyPath()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };
      
      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = createProjectEvent.ActionUTC.AddHours(1)
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      // this one ignores the IsArchived flag in DB
      g = projectRepo.GetProject_UnitTests(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      
      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Delete Project permanentlhy 
    /// </summary>
    [Fact]
    public void DeleteProject_Permanently()
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        DeletePermanently = true,
        ActionUTC = actionUtc.AddHours(1)
      };

      projectRepo.StoreEvent(createProjectEvent).Wait();
    
      // this one ignores the IsArchived flag in DB
      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.False(g.Result.IsArchived);

      var s = projectRepo.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Delete Project - ActionUTC too old
     ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [Fact]
    public void DeleteProject_OldActionUTC()
    {
      var ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
           
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
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = ActionUTC
      };

      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = ActionUTC.AddHours(-1)
      };

      var g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      var s = projectRepo.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.Equal(0, s.Result);

      g = projectRepo.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      CompareProject(g.Result,
        createProjectEvent.ProjectUID.ToString(), createProjectEvent.CustomerUID.ToString(), 0,
        createProjectEvent.ProjectName, createProjectEvent.Description, createProjectEvent.ProjectType,
        createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate,
        createProjectEvent.ProjectTimezone, ProjectTimezones.PacificAuckland,
        createProjectEvent.ProjectBoundary, createProjectEvent.ActionUTC, createProjectEvent.CoordinateSystemFileName);

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    #endregion Projects
    

    #region AssociateProjectWithGeofence

    /// <summary>
    /// Associate Project with Geofence - Happy Path
    /// </summary>
    [Fact]
    public void AssociateProjectWithGeofence_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();
      var boundary =
        "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))";

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
        ProjectBoundary = boundary,
        ActionUTC = actionUtc
      };

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Project.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = boundary,
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofence = new AssociateProjectGeofence
      {
        ProjectUID = createProjectEvent.ProjectUID,
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ReceivedUTC = actionUtc,
        ActionUTC = actionUtc
      };

      var geo = geofenceRepo.StoreEvent(createGeofenceEvent);
      geo.Wait();
      Assert.Equal(1, geo.Result);


      //var p = projectRepo.StoreEvent(createProjectEvent);
      //p.Wait();
      //Assert.Equal(1, p.Result);

      var pg = projectRepo.StoreEvent(associateProjectGeofence);
      pg.Wait();
      Assert.Equal(1, pg.Result);

      var g = projectRepo.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Single(g.Result);
      var retrievedPg = g.Result.FirstOrDefault();
      Assert.NotNull(retrievedPg);
      Assert.Equal(createProjectEvent.ProjectUID.ToString(), retrievedPg.ProjectUID);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), retrievedPg.GeofenceUID);
      Assert.Equal(createGeofenceEvent.GeofenceType, retrievedPg.GeofenceType.ToString());
    }

    /// <summary>
    /// Associate Project Geofence - then dissociate it
    /// </summary>
    [Fact]
    public void DissociateProjectGeofence_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = new Guid(customerUid),
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        ProjectUID = new Guid(projectUid),
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      geofenceRepo.StoreEvent(createGeofenceEvent).Wait();
      projectRepo.StoreEvent(associateProjectGeofenceEvent).Wait();

      var p = projectRepo.GetAssociatedGeofences(projectUid);
      p.Wait();
      var projectGeofences = p.Result.ToList();
      Assert.Single(projectGeofences);
      Assert.Equal(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID);

      var dissociateProjectGeofenceEvent = new DissociateProjectGeofence()
      {
        ProjectUID = new Guid(projectUid),
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(2)
      };

      projectRepo.StoreEvent(dissociateProjectGeofenceEvent).Wait();

      p = projectRepo.GetAssociatedGeofences(projectUid);
      p.Wait();
      projectGeofences = p.Result.ToList();
      Assert.Empty(projectGeofences);
    }

    #endregion AssociateProjectWithGeofence


    #region private

    private void CompareProject(Project result, 
      string expectedProjectUID, string expectedCustomerUID, int greaterThanShortRaptorProjectId,
      string expectedProjectName, string expectedDescription, ProjectType expectedProjectType,
      DateTime expectedProjectStartDate, DateTime expectedProjectEndDate, 
      string expectedProjectTimezone, string expectedProjectTimeZoneIana,
      string expectedBoundary, DateTime expectedActionUTC, string expectedCoordinateSystemFileName
      )
    {
      Assert.NotNull(result);
      Assert.Equal(expectedProjectUID, result.ProjectUID);
      Assert.Equal(expectedCustomerUID, result.CustomerUID);
      Assert.True(result.ShortRaptorProjectId > greaterThanShortRaptorProjectId);
      Assert.Equal(expectedProjectName, result.Name);
      Assert.Equal(expectedDescription, result.Description);
      Assert.Equal(expectedProjectType, result.ProjectType);
      Assert.Equal(expectedProjectStartDate, result.StartDate);
      Assert.Equal(expectedProjectEndDate, result.EndDate);
      Assert.Equal(expectedProjectTimezone, result.ProjectTimeZone);
      Assert.Equal(expectedProjectTimeZoneIana, result.ProjectTimeZoneIana);
      Assert.Equal(expectedBoundary, result.Boundary);
      if (result.CoordinateSystemFileName != null)
      {
        Assert.Equal(expectedActionUTC, result.CoordinateSystemLastActionedUTC);
        Assert.Equal(expectedCoordinateSystemFileName, result.CoordinateSystemFileName);
      }
    }

    private void CheckProjectHistoryCount(string projectUid, int expectedCount)
    {
      var projectHistory = projectRepo.GetProjectHistory_UnitTests(projectUid);
      projectHistory.Wait();
      Assert.NotNull(projectHistory.Result);
      Assert.Equal(expectedCount, projectHistory.Result.Count());
    }
    #endregion private
  }
}

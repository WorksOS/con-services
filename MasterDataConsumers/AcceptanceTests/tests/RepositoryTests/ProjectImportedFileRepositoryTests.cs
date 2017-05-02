using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using log4netExtensions;
using VSS.GenericConfiguration;
using Repositories;
using Repositories.DBModels;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectImportedFileRepositoryTests
  {
    IServiceProvider _serviceProvider = null;
    CustomerRepository _customerContext = null;
    ProjectRepository _projectContext = null;

    [TestInitialize]
    public void Init()
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      _serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      _customerContext = new CustomerRepository(_serviceProvider.GetService<IConfigurationStore>(), _serviceProvider.GetService<ILoggerFactory>());
      _projectContext = new ProjectRepository(_serviceProvider.GetService<IConfigurationStore>(), _serviceProvider.GetService<ILoggerFactory>());
      new SubscriptionRepository(_serviceProvider.GetService<IConfigurationStore>(), _serviceProvider.GetService<ILoggerFactory>());
    }


    #region ImportedFiles

    /// <summary>
    /// Create ImportedFile - Happy path i.e. 
    ///   ImportedFile doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateImportedFile_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
     
      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var s =_projectContext.StoreEvent(createImportedFileEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ImportedFile event not written");

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ImportedFile from ProjectRepo");

      var gList = _projectContext.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles from ProjectRepo");
      Assert.AreEqual(1, gList.Result.Count(), "ImportedFile count is incorrect from ProjectRepo");
    }

    /// <summary>
    /// Create ImportedFile - HappyPath 
    ///   ImportedFiles for multiple projects, should only return those for requested project.
    /// </summary>
    [TestMethod]
    public void CreateImportedFile_MultiProjects()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var project1 = Guid.NewGuid();
      var project2 = Guid.NewGuid();
      var project3 = Guid.NewGuid();

      var createImportedFileEventP1_1 = new CreateImportedFileEvent()
      {
        ProjectUID = project1,
        ImportedFileUID = Guid.NewGuid(),
        CustomerUID = customerUid,
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var createImportedFileEventP1_2 = new CreateImportedFileEvent()
      {
        ProjectUID = project1,
        ImportedFileUID = Guid.NewGuid(),
        CustomerUID = customerUid,
        ImportedFileType = ImportedFileType.Alignment,
        Name = "Test alginment type.svl",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var createImportedFileEventP2_1 = new CreateImportedFileEvent()
      {
        ProjectUID = project2,
        ImportedFileUID = Guid.NewGuid(),
        CustomerUID = customerUid,
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };


      _projectContext.StoreEvent(createImportedFileEventP1_1).Wait();
      _projectContext.StoreEvent(createImportedFileEventP1_2).Wait();
      _projectContext.StoreEvent(createImportedFileEventP2_1).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEventP1_2.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ImportedFile from ProjectRepo");

      var gList = _projectContext.GetImportedFiles(project1.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles for Project1 from ProjectRepo");
      Assert.AreEqual(2, gList.Result.Count(), "ImportedFile count is incorrect for Project1 from ProjectRepo");

      gList = _projectContext.GetImportedFiles(project2.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles for Project2 from ProjectRepo");
      Assert.AreEqual(1, gList.Result.Count(), "ImportedFile count is incorrect for Project2 from ProjectRepo");

      gList = _projectContext.GetImportedFiles(project3.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles for Project3 from ProjectRepo");
      Assert.AreEqual(0, gList.Result.Count(), "ImportedFile count is incorrect for Project3 from ProjectRepo");
    }

    /// <summary>
    /// Update ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [TestMethod]
    public void UpdateImportedFile_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      _projectContext.StoreEvent(createImportedFileEvent).Wait();
      
      var updateImportedFileEvent = new UpdateImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      _projectContext.StoreEvent(updateImportedFileEvent).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ImportedFile from ProjectRepo");
      Assert.AreEqual(updateImportedFileEvent.ActionUTC, g.Result.LastActionedUtc, "ImportedFile actionUtc was not updated");
    }

    /// <summary>
    /// Delete ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [TestMethod]
    public void DeleteImportedFile_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      _projectContext.StoreEvent(createImportedFileEvent).Wait();

      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      _projectContext.StoreEvent(deleteImportedFileEvent).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve ImportedFile from ProjectRepo");
    }

    #endregion ImportedFiles


    #region Private
    private CreateProjectEvent CopyModel(Project project)
    {
      return new CreateProjectEvent()
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        ProjectID = project.LegacyProjectID,
        ProjectName = project.Name,
        ProjectType = project.ProjectType,
        ProjectTimezone = project.ProjectTimeZone,

        ProjectStartDate = project.StartDate,
        ProjectEndDate = project.EndDate,
        ActionUTC = project.LastActionedUTC,
        ProjectBoundary = project.GeometryWKT,
        CoordinateSystemFileName = project.CoordinateSystemFileName
      };
    }

    private Repositories.DBModels.Project CopyModel(CreateProjectEvent kafkaProjectEvent)
    {
      return new Project()
      {
        ProjectUID = kafkaProjectEvent.ProjectUID.ToString(),
        LegacyProjectID = kafkaProjectEvent.ProjectID,
        Name = kafkaProjectEvent.ProjectName,
        ProjectType = kafkaProjectEvent.ProjectType,
        // IsDeleted =  N/A

        ProjectTimeZone = kafkaProjectEvent.ProjectTimezone,
        LandfillTimeZone = TimeZone.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate,
        GeometryWKT = kafkaProjectEvent.ProjectBoundary,
        CoordinateSystemFileName = kafkaProjectEvent.CoordinateSystemFileName
      };
    }
    #endregion Private

  }
}
 
 
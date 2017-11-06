using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Scheduler.Common.Models;
using Moq;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class ImportedFileTests : BaseTests
  {
    protected ILogger _log;

    [TestMethod]
    public void GetImportedFileFromProject_NoneExists()
    {
      _log = _logger.CreateLogger<ImportedFileTests>();

      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(_configStore, _log, "_PROJECT");
      Assert.AreEqual(
        "server=localhost;port=3306;database=VSS-Productivity3D-Project;userid=root;password=abc123;Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4",
        projectDbConnectionString, "incorrect project dbConnectionString");
      var importedFileHandlerProject = new ImportedFileHandlerProject<ProjectImportedFile>(_configStore, _logger);
      Assert.IsNotNull(importedFileHandlerProject, "unable to createimportedFileHandlerProject");

      var listOfProjectFiles = importedFileHandlerProject.List();
      Assert.IsNotNull(listOfProjectFiles, "should be valid list");
      Assert.AreEqual(0, listOfProjectFiles.Count, "should not be any files");
    }


    [TestMethod]
    [Ignore] // todo Moq
    public void GetImportedFileFromProject_OneExists()
    {
      _log = _logger.CreateLogger<ImportedFileTests>();

      string projectDbConnectionString = ConnectionUtils.GetConnectionStringMySql(_configStore, _log, "_PROJECT");
      var importedFileHandlerProject = new ImportedFileHandlerProject<ProjectImportedFile>(_configStore, _logger);
      var readCount = importedFileHandlerProject.Read();
      Assert.AreEqual(1, readCount, "should have been 1 file written");

      var listOfProjectFiles = importedFileHandlerProject.List();
      Assert.IsNotNull(listOfProjectFiles, "should be valid list");
      Assert.AreEqual(1, listOfProjectFiles.Count, "should be 1 file");
    }
  }
}

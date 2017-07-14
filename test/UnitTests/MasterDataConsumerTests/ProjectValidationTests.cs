using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Project.WebAPI.Common.Models;

namespace VSS.MasterData.ConsumerTests
{
  [TestClass]
  public class ProjectValidationTests
  {
    private ProjectTimezone _projectTimezone;

    [TestInitialize]
    public void TestInitialize()
    {
      _projectTimezone = new ProjectTimezone();
    }

    [TestMethod]
    public void ValidateCreateProject_InvalidProjectTimeZone()
    {
      Assert.IsFalse(_projectTimezone.timeZone.Contains("whatever"), "ProjectTimezone should be invalid");
    }

    [TestMethod]
    public void ValidateCreateProject_ValidProjectTimeZone()
    {
      Assert.IsTrue(_projectTimezone.timeZone.Contains("Namibia Standard Time"), "ProjectTimezone should be valid");
    }

    [TestMethod]
    public void ValidateCreateProject_ValidProjectTimeZoneCaseSensitive()
    {
      Assert.IsFalse(_projectTimezone.timeZone.Contains("Namibia sTandard Time"), "ProjectTimezone should be correct case");
    }

    [TestMethod]
    public void ValidateCreateProject_InValidProjectTimeZone()
    {
      Assert.IsFalse(_projectTimezone.timeZone.Contains("Namibia Standard Time Namibia Standard Time"), "ProjectTimezone should be invalid");
    }
  }
}
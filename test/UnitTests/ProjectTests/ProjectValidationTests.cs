using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.ProjectWebApiCommon.Models;

namespace ProjectTests
{
  [TestClass]
  public class ProjectValidationTests
  {
    [TestMethod]
    public void ValidateCreateProject_InvalidProjectTimeZone()
    {
      ProjectTimezone projectTimezone = new ProjectTimezone();
      Assert.IsFalse(projectTimezone.timeZone.Contains("whatever"), "ProjectTimezone should be invalid");
    }
    
    [TestMethod]
    public void ValidateCreateProject_ValidProjectTimeZone()
    {
      ProjectTimezone projectTimezone = new ProjectTimezone();
      Assert.IsTrue(projectTimezone.timeZone.Contains("Namibia Standard Time"), "ProjectTimezone should be valid");
    }

    [TestMethod]
    public void ValidateCreateProject_ValidProjectTimeZoneCaseSensitive()
    {
      ProjectTimezone projectTimezone = new ProjectTimezone();
      Assert.IsFalse(projectTimezone.timeZone.Contains("Namibia sTandard Time"), "ProjectTimezone should be correct case");
    }

    [TestMethod]
    public void ValidateCreateProject_InValidProjectTimeZone()
    {
      ProjectTimezone projectTimezone = new ProjectTimezone();
      Assert.IsFalse(projectTimezone.timeZone.Contains("Namibia Standard Time Namibia Standard Time"), "ProjectTimezone should be invalid");
    }

  }
}

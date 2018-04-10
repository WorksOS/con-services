using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.ExtendedModels;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectTimezoneValidationTests
  {
    private List<string> _projectTimezones;

    [TestInitialize]
    public void TestInitialize()
    {
      _projectTimezones = PreferencesTimeZones.WindowsTimeZoneNames().ToList();
    }

    [TestMethod]
    public void ValidateCreateProject_InvalidProjectTimeZone()
    {
      Assert.IsFalse(_projectTimezones.Contains("whatever"), "ProjectTimezone should be invalid");
    }

    [TestMethod]
    public void ValidateCreateProject_ValidProjectTimeZone()
    {
      Assert.IsTrue(_projectTimezones.Contains("Namibia Standard Time"), "ProjectTimezone should be valid");
    }

    [TestMethod]
    public void ValidateCreateProject_ValidProjectTimeZoneCaseSensitive()
    {
      Assert.IsFalse(_projectTimezones.Contains("Namibia sTandard Time"), "ProjectTimezone should be correct case");
    }

    [TestMethod]
    public void ValidateCreateProject_InValidProjectTimeZone()
    {
      Assert.IsFalse(_projectTimezones.Contains("Namibia Standard Time Namibia Standard Time"), "ProjectTimezone should be invalid");
    }

    [TestMethod]
    public void WindowsToIana_MissingWindowsTimeZone()
    {
      Assert.AreEqual(string.Empty, PreferencesTimeZones.WindowsToIana(null), "IANA time zone should be empty string");
    }

    [TestMethod]
    public void WindowsToIana_InvalidWindowsTimeZone()
    {
      Assert.IsNull(PreferencesTimeZones.WindowsToIana("NZST"), "IANA time zone should be null");
    }

    [TestMethod]
    public void WindowsToIana_ValidWindowsTimeZone()
    {
      Assert.AreEqual("Pacific/Auckland", PreferencesTimeZones.WindowsToIana("New Zealand Standard Time"), "IANA time zone should be correct");
    }

    [TestMethod]
    public void IanaToWindows_MissingIanaTimeZone()
    {
      Assert.AreEqual(string.Empty, PreferencesTimeZones.IanaToWindows(null), "Windows time zone should be empty string");
    }

    [TestMethod]
    public void IanaToWindows_InvalidIanaTimeZone()
    {
      Assert.IsNull(PreferencesTimeZones.IanaToWindows("Pacific/Wellington"), "Windows time zone should be null");
    }

    [TestMethod]
    public void IanaToWindows_ValidIanaTimeZone()
    {
      Assert.AreEqual("New Zealand Standard Time", PreferencesTimeZones.IanaToWindows("Pacific/Auckland"), "Windows time zone should be correct");
    }
  }
}
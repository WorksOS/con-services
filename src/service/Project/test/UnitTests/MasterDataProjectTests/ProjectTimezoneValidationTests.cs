using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Repositories.ExtendedModels;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectTimezoneValidationTests
  {
    private readonly List<string> _projectTimezones = PreferencesTimeZones.WindowsTimeZoneNames().ToList();

    [Fact]
    public void ValidateCreateProject_InvalidProjectTimeZone()
    {
      Assert.False(_projectTimezones.Contains("whatever"), "ProjectTimezone should be invalid");
    }

    [Fact]
    public void ValidateCreateProject_ValidProjectTimeZone()
    {
      Assert.True(_projectTimezones.Contains("Namibia Standard Time"), "ProjectTimezone should be valid");
    }

    [Fact]
    public void ValidateCreateProject_ValidProjectTimeZoneCaseSensitive()
    {
      Assert.DoesNotContain("Namibia sTandard Time", _projectTimezones);
    }

    [Fact]
    public void ValidateCreateProject_InValidProjectTimeZone()
    {
      Assert.DoesNotContain("Namibia Standard Time Namibia Standard Time", _projectTimezones);
    }

    [Fact]
    public void WindowsToIana_MissingWindowsTimeZone()
    {
      Assert.Equal(string.Empty, PreferencesTimeZones.WindowsToIana(null));
    }

    [Fact]
    public void WindowsToIana_InvalidWindowsTimeZone()
    {
      Assert.Null(PreferencesTimeZones.WindowsToIana("NZST"));
    }

    [Fact]
    public void WindowsToIana_ValidWindowsTimeZone()
    {
      Assert.Equal("Pacific/Auckland", PreferencesTimeZones.WindowsToIana("New Zealand Standard Time"));
    }

    [Fact]
    public void IanaToWindows_MissingIanaTimeZone()
    {
      Assert.Equal(string.Empty, PreferencesTimeZones.IanaToWindows(null));
    }

    [Fact]
    public void IanaToWindows_InvalidIanaTimeZone()
    {
      Assert.Null(PreferencesTimeZones.IanaToWindows("Pacific/Wellington"));
    }

    [Fact]
    public void IanaToWindows_ValidIanaTimeZone()
    {
      Assert.Equal("New Zealand Standard Time", PreferencesTimeZones.IanaToWindows("Pacific/Auckland"));
    }
  }
}

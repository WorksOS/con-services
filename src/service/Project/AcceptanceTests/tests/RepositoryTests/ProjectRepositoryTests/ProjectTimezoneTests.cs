using RepositoryTests.Internal;
using VSS.MasterData.Repositories.ExtendedModels;
using Xunit;

namespace RepositoryTests.ProjectRepositoryTests
{
  public class ProjectTimezoneTests
  {
    /// <summary>
    /// These Timezone conversions need to be done as acceptance tests so they are run on linux - the target platform.
    /// They should behave ok on windows (this will occur if you run a/ts against local containers).
    /// </summary>
    [Fact]
    public void ConvertTimezone_WindowsToIana()
    {
      Assert.Equal("Pacific/Auckland", PreferencesTimeZones.WindowsToIana(ProjectTimezones.NewZealandStandardTime));
    }

    [Fact]
    public void ConvertTimezone_WindowsToIana_Invalid()
    {
      Assert.Null(PreferencesTimeZones.WindowsToIana("New Zealand Standard Time222"));
    }

    [Fact]
    public void ConvertTimezone_WindowsToIana_alreadyIana()
    {
      Assert.Null(PreferencesTimeZones.WindowsToIana(ProjectTimezones.PacificAuckland));
    }

    [Fact]
    public void ConvertTimezone_WindowsToIana_UTC()
    {
      Assert.Equal("Etc/GMT", PreferencesTimeZones.WindowsToIana(ProjectTimezones.Utc));
    }
  }
}

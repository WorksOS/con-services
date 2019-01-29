using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class ExportRequestHelperTests
  {
    /*
     * These tests looks a bit stange with no assertions, basically it tests that no exception
     * is thrown when converting an empty UserPreferenceData.
     */
    [TestMethod]
    public void ConvertEmptyPreferences()
    {
      var converted = ExportRequestHelper.ConvertToRaptorUserPreferences(new UserPreferenceData(), "Aleutian Standard Time");
    }


    [TestMethod]
    public void ConvertPreferencesWithtimezoneButNoProjectTimeZone()
    {
      _ = ExportRequestHelper.ConvertToRaptorUserPreferences(new UserPreferenceData
      {
        Timezone = "Aleutian Standard Time"
      },
      null);
    }
  }
}

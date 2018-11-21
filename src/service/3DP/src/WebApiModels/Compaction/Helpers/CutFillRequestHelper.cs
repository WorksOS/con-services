using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Helper to create a cut-fill details request.
  /// </summary>
  public class CutFillRequestHelper : DataRequestBase, ICutFillRequestHelper
  {
    public CutFillRequestHelper()
    { }

    /// <summary>
    /// Creates an instance of the CutFillDetailsRequest class and populate it with data needed for a cut-fill details request.   
    /// </summary>
    /// <returns>An instance of the CutFillDetailsRequest class.</returns>
    public CutFillDetailsRequest Create()
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);
      var cutFillSettings = SettingsManager.CompactionCutFillSettings(ProjectSettings);
      return new CutFillDetailsRequest(ProjectId, ProjectUid, cutFillSettings, Filter, liftSettings, DesignDescriptor);
    }
  }
}

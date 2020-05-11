using System.IO;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace CCSS.CWS.Client
{
  public class ProjectConfigurationFileHelper
  {
    /// <summary>
    /// Returns true if the imported file type is a CWS project configuration file type
    /// </summary>
    public static bool isCwsFileType(ImportedFileType importedFileType)
    {
      switch (importedFileType)
      {
        case ImportedFileType.Calibration:
        case ImportedFileType.AvoidanceZone:
        case ImportedFileType.ControlPoints:
        case ImportedFileType.Geoid:
        case ImportedFileType.FeatureCode:
        case ImportedFileType.SiteConfiguration:
        case ImportedFileType.GcsCalibration:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Determines if the file is a site collector or machine control type
    /// </summary>
    public static bool isSiteCollectorType(ImportedFileType importedFileType, string filename)
    {
      var fileExtension = Path.GetExtension(filename).ToLower();
      return ((importedFileType == ImportedFileType.ControlPoints && fileExtension == ".csv") ||
              (importedFileType == ImportedFileType.AvoidanceZone && fileExtension == ".dxf"));
    }
  }
}

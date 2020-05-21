using System.IO;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace CCSS.CWS.Client
{
  public class ProjectConfigurationFileHelper
  {
    /// <summary>
    /// Returns true if the imported file type is a CWS project configuration file type
    /// </summary>
    public static bool IsCwsFileType(ImportedFileType importedFileType)
    {
      switch (importedFileType)
      {
        case ImportedFileType.CwsCalibration:
        case ImportedFileType.CwsAvoidanceZone:
        case ImportedFileType.CwsControlPoints:
        case ImportedFileType.CwsGeoid:
        case ImportedFileType.CwsFeatureCode:
        case ImportedFileType.CwsSiteConfiguration:
        case ImportedFileType.CwsGcsCalibration:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Determines if the file is a site collector or machine control type
    /// </summary>
    public static bool IsSiteCollectorType(ImportedFileType importedFileType, string filename)
    {
      var fileExtension = Path.GetExtension(filename).ToLower();
      return ((importedFileType == ImportedFileType.CwsControlPoints && fileExtension == ".csv") ||
              (importedFileType == ImportedFileType.CwsAvoidanceZone && fileExtension == ".dxf"));
    }
  }
}

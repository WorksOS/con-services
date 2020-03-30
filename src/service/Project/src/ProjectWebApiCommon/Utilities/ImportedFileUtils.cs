using System.IO;
using System.Net;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  public static class ImportedFileUtils
  {
    public static string RemoveSurveyedUtcFromName(string name)
    {
      var shortFileName = Path.GetFileNameWithoutExtension(name);
      var format = "yyyy-MM-ddTHHmmssZ";
      if (!DoesNameIncludeUtc(shortFileName, format))
        return name;
      return shortFileName.Substring(0, shortFileName.Length - format.Length - 1) + Path.GetExtension(name);
    }

    private static bool DoesNameIncludeUtc(string name, string format)
    {
      if (name.Length <= format.Length)
        return false;
      var endOfName = name.Substring(name.Length - format.Length);
      var pattern = "^\\d{4}-\\d{2}-\\d{2}T\\d{6}Z$";
      var isMatch = (System.Text.RegularExpressions.Regex.IsMatch(endOfName, pattern,
        System.Text.RegularExpressions.RegexOptions.IgnoreCase)) ;
      return isMatch;
    }

    public static void ValidateEnvironmentVariables(ImportedFileType importedFileType, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler)
    {
      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"), out var useRaptorGatewayDesignImport);
      var isDesignFileType = importedFileType == ImportedFileType.DesignSurface ||
                             importedFileType == ImportedFileType.SurveyedSurface ||
                             importedFileType == ImportedFileType.Alignment ||
                             importedFileType == ImportedFileType.ReferenceSurface;
      if (!useRaptorGatewayDesignImport &&
          !(useTrexGatewayDesignImport && isDesignFileType))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 113); 
      }
    }
  }
}

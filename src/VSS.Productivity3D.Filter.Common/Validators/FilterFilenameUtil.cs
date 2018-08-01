using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Models;

namespace VSS.Productivity3D.Filter.Common.Validators
{
  public class FilterFilenameUtil
  {
    public static void GetFilterFileNames(ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileListProxy fileListProxy, FilterRequestFull filterRequestFull)
    {
      log.LogInformation($"{nameof(GetFilterFileNames)}: Resolving filenames for Filter JSON.");

      var tmpFilter = filterRequestFull.FilterModel(serviceExceptionHandler);

      if (string.IsNullOrEmpty(tmpFilter.DesignUid) && string.IsNullOrEmpty(tmpFilter.AlignmentUid))
      {
        log.LogInformation($"{nameof(GetFilterFileNames)}: No filenames to resolve.");
        return;
      }

      var fileList = fileListProxy.GetFiles(filterRequestFull.ProjectUid, filterRequestFull.UserId, filterRequestFull.CustomHeaders).Result;
      if (fileList == null || fileList.Count == 0)
      {
        return;
      }

      if (!string.IsNullOrEmpty(tmpFilter.DesignUid))
      {
        tmpFilter.SetFilenames(designName: fileList.FirstOrDefault(data => data.ImportedFileUid == tmpFilter.DesignUid)?.Name);
        log.LogInformation($"{nameof(GetFilterFileNames)}: Resolved: {tmpFilter.DesignName}");
      }

      if (!string.IsNullOrEmpty(tmpFilter.AlignmentUid))
      {
        tmpFilter.SetFilenames(alignmentName: fileList.FirstOrDefault(data => data.ImportedFileUid == tmpFilter.AlignmentUid)?.Name);
        log.LogInformation($"{nameof(GetFilterFileNames)}: Resolved: {tmpFilter.AlignmentName}");
      }

      filterRequestFull.FilterJson = tmpFilter.ToJsonString();
    }
  }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Extensions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger Log;
    protected IConfigurationStore ConfigurationStore;
    protected IFileListProxy FileListProxy;
    protected ICompactionSettingsManager SettingsManager;

    protected long ProjectId;
    protected IDictionary<string, string> Headers;
    protected CompactionProjectSettings ProjectSettings;
    protected List<long> ExcludedIds;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, long projectId, CompactionProjectSettings projectSettings, IDictionary<string, string> headers, List<long> excludeIds)
    {
      Log = log;
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;

      ProjectId = projectId;
      Headers = headers;
      ProjectSettings = projectSettings;
      ExcludedIds = excludeIds;
    }

    protected DesignDescriptor GetDescriptor(Guid projectUid, Guid importedFileUid)
    {
      var fileList = FileListProxy.GetFiles(projectUid.ToString(), Headers).Result;

      if (fileList.Count <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Project has no appropriate design files."));
      }

      var designFile = fileList.SingleOrDefault(
        f => f.ImportedFileUid ==
             importedFileUid.ToString() &&
             f.IsActivated &&
             f.IsSupportedFileType());

      if (designFile == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to access design file."));
      }

      return DesignDescriptor.CreateDesignDescriptor(
        designFile.LegacyFileId,
        FileDescriptor.CreateFileDescriptor(GetFilespaceId(), designFile.Path, designFile.Name),
        0);
    }
    
    private string GetFilespaceId()
    {
      var filespaceId = ConfigurationStore.GetValueString("TCCFILESPACEID");
      if (!string.IsNullOrEmpty(filespaceId))
      {
        return filespaceId;
      }

      const string errorString = "Your application is missing an environment variable TCCFILESPACEID";
      Log.LogError(errorString);
      throw new InvalidOperationException(errorString);
    }
  }
}
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CCSS.Productivity3D.Service.Common.Enums;
using CCSS.Productivity3D.Service.Common.Extensions;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Extensions;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace CCSS.Productivity3D.Service.Common
{
  public static class DesignUtilities
  {
    /// <summary>
    /// Gets the <see cref="DesignDescriptor"/> from a given project's fileUid.
    /// </summary>
    public static async Task<DesignDescriptor> GetAndValidateDesignDescriptor(
      Guid projectUid, Guid? fileUid, string userUid, IHeaderDictionary customHeaders, 
      IFileImportProxy fileImportProxy, IConfigurationStore configStore, ILogger log, OperationType operation = OperationType.General)
    {
      if (!fileUid.HasValue)
      {
        return null;
      }

      var fileList = await fileImportProxy.GetFiles(projectUid.ToString(), userUid, customHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Project has no appropriate design files."));
      }

      FileData file = null;

      foreach (var f in fileList)
      {
        bool operationSupported = true;
        switch (operation)
        {
          case OperationType.Profiling:
            operationSupported = f.IsProfileSupportedFileType();
            break;
          default:
            //All file types supported
            break;
        }
        if (f.ImportedFileUid == fileUid.ToString() && f.IsActivated && operationSupported)
        {
          file = f;

          break;
        }
      }

      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to access design or alignment file."));
      }

      var tccFileName = file.Name;
      if (file.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        //Note: ':' is an invalid character for filenames in Windows so get rid of them
        tccFileName = Path.GetFileNameWithoutExtension(tccFileName) +
                      "_" + file.SurveyedUtc.Value.ToIso8601DateTimeString().Replace(":", string.Empty) +
                      Path.GetExtension(tccFileName);
      }

      string fileSpaceId = configStore.GetValueString("TCCFILESPACEID");

      if (string.IsNullOrEmpty(fileSpaceId))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACEID";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, file.Path, tccFileName);

      return new DesignDescriptor(file.LegacyFileId, fileDescriptor, file.Offset ?? 0.0, fileUid);
    }
  }
}

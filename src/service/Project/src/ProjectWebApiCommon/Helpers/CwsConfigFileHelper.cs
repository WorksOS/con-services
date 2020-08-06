using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public static class CwsConfigFileHelper
  {
    /// <summary>
    /// Upserts a project configuration file to CWS.
    /// </summary>
    public static async Task<ProjectConfigurationModel> SaveProjectConfigurationFileToCws(Guid projectUid, string filename, Stream fileContents,
      ICwsDesignClient cwsDesignClient, ICwsProfileSettingsClient cwsProfileSettingsClient, IHeaderDictionary customHeaders)
    {
      // use User token for CWS. If app token required use auth.CustomHeaders() 
      var existingFileTask = GetCwsFile(projectUid, cwsProfileSettingsClient, customHeaders);
      var createAndUploadTask = cwsDesignClient.CreateAndUploadFile(projectUid, new CreateFileRequestModel { FileName = filename }, fileContents, customHeaders);
      var tasks = new List<Task> { existingFileTask, createAndUploadTask };
      await Task.WhenAll(tasks);
      var request = new ProjectConfigurationFileRequestModel();
      request.MachineControlFilespaceId = createAndUploadTask?.Result.FileSpaceId;

      var configResult = await(existingFileTask?.Result == null ?
        cwsProfileSettingsClient.SaveProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION, request, customHeaders) :
        cwsProfileSettingsClient.UpdateProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION, request, customHeaders));
      return configResult;
    }

    ///// <summary>
    ///// Gets a project configuration file from CWS which has the given file name associated with it.
    ///// Control points and avoidance zones can have 2 files associated with the configuration file, one for site collectors and one for machine control.
    ///// </summary>
    //public static async Task<ProjectConfigurationModel> GetCwsFile(Guid projectUid, string filename, ImportedFileType importedFileType,
    //  ICwsProfileSettingsClient cwsProfileSettingsClient, IHeaderDictionary customHeaders)
    //{
    //  var existingFile = await GetCwsFile(projectUid, importedFileType, cwsProfileSettingsClient, customHeaders);
    //  if (existingFile == null)
    //    return null;

    //  var matches = ProjectConfigurationFileHelper.IsSiteCollectorType(importedFileType, filename) ? existingFile.SiteCollectorFileName == filename : existingFile.FileName == filename;
    //  return matches? existingFile : null;
    //}

    /// <summary>
    /// Gets a project configuration file from CWS which has the given file type.
    /// Control points and avoidance zones can have 2 files associated with the configuration file, one for site collectors and one for machine control.
    /// </summary>
    public static Task<ProjectConfigurationModel> GetCwsFile(Guid projectUid, ICwsProfileSettingsClient cwsProfileSettingsClient, IHeaderDictionary customHeaders)
    {
      return cwsProfileSettingsClient.GetProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION, customHeaders);
    }
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CCSS.CWS.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.Handlers;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public class CwsConfigFileHelper
  {
    /*
      Config type	          Earthworks      GCS                 Siteworks           SCS
                            (EC520 and      (CB460, CB450, 	    (Tablet)            (TSC3, Mobile)
	                           EC520-W)       CB430, CD700)                       
	      
      CwsCalibration	          .cal	          .cfg (if present)	  .dc	                .dc
                                            or .cal
      CwsGeoid	                .ggf	          .ggf	              .ggf	              .ggf
      Control points	      .cpz		                            .office.csv/.csv 	  .office.csv/.csv 
      Avoidance zone	      .avoid.svl		                      .avoid.dxf	        .avoid.dxf
      Feature code library	.fxl		                            .fxl	              .fxl
      Site Configuration			                                  Site.xml	          Site.xml
     */
    private static readonly Dictionary<ImportedFileType, ProjectConfigurationFileType> _cwsFileTypeMap = new Dictionary<ImportedFileType, ProjectConfigurationFileType>
    {
      {ImportedFileType.CwsCalibration, ProjectConfigurationFileType.CALIBRATION},
      {ImportedFileType.CwsAvoidanceZone, ProjectConfigurationFileType.AVOIDANCE_ZONE},
      {ImportedFileType.CwsControlPoints, ProjectConfigurationFileType.CONTROL_POINTS},
      {ImportedFileType.CwsGeoid, ProjectConfigurationFileType.GEOID},
      {ImportedFileType.CwsFeatureCode, ProjectConfigurationFileType.FEATURE_CODE},
      {ImportedFileType.CwsSiteConfiguration, ProjectConfigurationFileType.SITE_CONFIGURATION},
      {ImportedFileType.CwsGcsCalibration, ProjectConfigurationFileType.GCS_CALIBRATION}
      // Is ProjectConfigurationFileType.SITE_MAP supported/used ?
    };

    /// <summary>
    /// Upserts a project configuration file to CWS.
    /// </summary>
    public static async Task<ProjectConfigurationFileResponseModel> SaveFileToCws(Guid projectUid, string filename, Stream fileContents,
      ImportedFileType importedFileType, ICwsDesignClient cwsDesignClient, ICwsProfileSettingsClient cwsProfileSettingsClient,
      IHeaderDictionary customHeaders)
    {
      // use User token for CWS. If app token required use auth.CustomHeaders() 
      var existingFileTask = GetCwsFile(projectUid, importedFileType, cwsProfileSettingsClient, customHeaders);
      var createAndUploadTask = cwsDesignClient.CreateAndUploadFile(projectUid, new CreateFileRequestModel { FileName = filename }, fileContents, customHeaders);
      var tasks = new List<Task> {existingFileTask, createAndUploadTask};
      await Task.WhenAll(tasks);
      var fileType = _cwsFileTypeMap[importedFileType];
      var request = new ProjectConfigurationFileRequestModel();
      if (ProjectConfigurationFileHelper.IsSiteCollectorType(importedFileType, filename))
      {
        request.SiteCollectorFilespaceId = createAndUploadTask?.Result.FileSpaceId;
      }
      else
      {
        request.MachineControlFilespaceId = createAndUploadTask?.Result.FileSpaceId;
      }

      var configResult = await(existingFileTask?.Result == null ?
        cwsProfileSettingsClient.SaveProjectConfiguration(projectUid, fileType, request, customHeaders) :
        cwsProfileSettingsClient.UpdateProjectConfiguration(projectUid, fileType, request, customHeaders));
      return configResult;
    }

    /// <summary>
    /// Gets a project configuration file from CWS which has the given file name associated with it.
    /// Control points and avoidance zones can have 2 files associated with the configuration file, one for site collectors and one for machine control.
    /// </summary>
    public static async Task<ProjectConfigurationFileResponseModel> GetCwsFile(Guid projectUid, string filename, ImportedFileType importedFileType,
      ICwsProfileSettingsClient cwsProfileSettingsClient, IHeaderDictionary customHeaders)
    {
      var existingFile = await GetCwsFile(projectUid, importedFileType, cwsProfileSettingsClient, customHeaders);
      if (existingFile == null)
        return null;

      var matches = ProjectConfigurationFileHelper.IsSiteCollectorType(importedFileType, filename) ? existingFile.SiteCollectorFileName == filename : existingFile.FileName == filename;
      return matches ? existingFile : null;
    }

    /// <summary>
    /// Gets a project configuration file from CWS which has the given file type.
    /// Control points and avoidance zones can have 2 files associated with the configuration file, one for site collectors and one for machine control.
    /// </summary>
    public static Task<ProjectConfigurationFileResponseModel> GetCwsFile(Guid projectUid, ImportedFileType importedFileType,
      ICwsProfileSettingsClient cwsProfileSettingsClient, IHeaderDictionary customHeaders)
    {
      var fileType = _cwsFileTypeMap[importedFileType];
      return cwsProfileSettingsClient.GetProjectConfiguration(projectUid, fileType, customHeaders);
    }

    /// <summary>
    /// Deletes a project configuration file from CWS.
    /// </summary>
    public static async Task DeleteFileFromCws(Guid projectUid, ImportedFileType importedFileType, string filename, ICwsDesignClient cwsDesignClient,
      ICwsProfileSettingsClient cwsProfileSettingsClient, IServiceExceptionHandler serviceExceptionHandler, IWebClientWrapper webClient, 
      IHeaderDictionary customHeaders, ILogger Logger)
    {
      var existingFile = await GetCwsFile(projectUid, importedFileType, cwsProfileSettingsClient, customHeaders);
      if (existingFile != null)
      {
        // If 2 files and filename provided then need to download the other file then delete the given file then upload and save the other file.
        // Otherwise just delete the project config file.
        bool twoFiles = !string.IsNullOrEmpty(existingFile.FileName) && !string.IsNullOrEmpty(existingFile.SiteCollectorFileName) && !string.IsNullOrEmpty(filename);
        string otherFilename = null;
        byte[] otherFileContents = null;
        if (twoFiles)
        {
          // Download the other file
          string downloadLink = null;
          
          if (existingFile.FileName == filename)
          {
            downloadLink = existingFile.SiteCollectorFileDownloadLink;
            otherFilename = existingFile.SiteCollectorFileName;
          }
          else if (existingFile.SiteCollectorFileName == filename)
          {
            downloadLink = existingFile.FileDownloadLink;
            otherFilename = existingFile.FileName;
          }
          else
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 56);
          }

          otherFileContents = webClient.DownloadData(downloadLink);
        }
        // Delete the CWS config
        await cwsProfileSettingsClient.DeleteProjectConfiguration(projectUid, _cwsFileTypeMap[importedFileType], customHeaders);
        if (twoFiles)
        {
          // Upload the other file. These are small files so should be ok to do in memory.
          using (var ms = new MemoryStream(otherFileContents))
          {
            await SaveFileToCws(projectUid, otherFilename, ms, importedFileType, cwsDesignClient, cwsProfileSettingsClient, customHeaders);
          }
        }
      }
      else {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 56);
      }
    }
  }
}

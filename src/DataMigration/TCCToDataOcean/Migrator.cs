using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean
{
  public class Migrator
  {
    private readonly IProjectRepository ProjectRepo;
    private readonly IServiceExceptionHandler ServiceExceptionHandler;
    private readonly IFileRepository FileRepo;
    private readonly IWebApiUtils WebApiUtils;
    private readonly IImportFile ImportFile;
    private readonly ILogger Log;
    private string FileSpaceId;
    private readonly string BaseUrl;


    private const string ProjectWebApiKey = "PROJECT_API_URL";
    private const string TccFilespaceKey = "TCCFILESPACEID";


    public Migrator(IProjectRepository projectRepository, IConfigurationStore configStore, 
      IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo,
      ILoggerFactory loggerFactory, IWebApiUtils webApiUtils, IImportFile importFile)
    {
      ProjectRepo = projectRepository;
      FileRepo = fileRepo;
      ServiceExceptionHandler = serviceExceptionHandler;
      WebApiUtils = webApiUtils;
      ImportFile = importFile;
      FileSpaceId = configStore.GetValueString(TccFilespaceKey);
      if (string.IsNullOrEmpty(FileSpaceId))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48,
          $"Missing environment variable {TccFilespaceKey}");
      }
      BaseUrl = configStore.GetValueString(ProjectWebApiKey);
      if (string.IsNullOrEmpty(BaseUrl))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48,
          $"Missing environment variable {ProjectWebApiKey}");
      }

      Log = loggerFactory.CreateLogger<Migrator>();
    }

    public async Task MigrateFiles()
    {
      var projectUri = $"{BaseUrl}api/v4/project/";
      //Get list of all active projects for all customers
      var projects = await ProjectRepo.GetActiveProjects();
      foreach (var project in projects)
      {
        //Update project to get coordinate system file migrated
        var updateProjectResult = WebApiUtils.UpdateProjectViaWebApi(projectUri, project);
        //Get list of imported files for project from project web api
        var fileUriRoot = $"{BaseUrl}api/v4/importedfiles";
        var filesResult = ImportFile.GetImportedFilesFromWebApi($"{fileUriRoot}?projectUid={project.ProjectUID}", project.CustomerUID);
        foreach (var file in filesResult.ImportedFileDescriptors)
        {
          if (file.ImportedFileType == ImportedFileType.Linework ||
              file.ImportedFileType == ImportedFileType.DesignSurface ||
              file.ImportedFileType == ImportedFileType.SurveyedSurface ||
              file.ImportedFileType == ImportedFileType.Alignment)
          {
            //Download file from TCC
            var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}");
            //TODO: use a env var for temp folder
            //TODO: make the whole process async - then will require unique temp name so no clashes in temp folder with multiple downloads
            var tempFolder = @"C:/MyOutput.pdf";
            var tempFileName = $"{tempFolder}/{file.Name}";
            using (FileStream tempFile = new FileStream(tempFileName, FileMode.Create))
            {
              fileContents.CopyTo(tempFile);
            }            
            //Upload to project web api to migrate
            //TODO: file filename will be downloaded temporary file I think - check
            var updateFileResult = ImportFile.SendRequestToFileImportV4(fileUriRoot, file, tempFileName, new ImportOptions(HttpMethod.Post));
          }
        }
      }
    } 
  }
}

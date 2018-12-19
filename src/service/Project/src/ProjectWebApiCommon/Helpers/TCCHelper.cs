using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public class TccHelper
  {
    /// <summary>
    /// get file content from TCC
    ///     note that is is intended to be used for small, DC files only.
    ///     If/when it is needed for large files, 
    ///           e.g. surfaces, you should use a smaller buffer and loop to read.
    /// </summary>
    public static async Task<byte[]> GetFileContentFromTcc(BusinessCenterFile businessCentreFile,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      Stream memStream = null;
      var tccPath = $"{businessCentreFile.Path}/{businessCentreFile.Name}";
      byte[] coordSystemFileContent = null;
      int numBytesRead = 0;

      try
      {
        log.LogInformation(
          $"GetFileContentFromTcc: getBusinessCentreFile fielspaceID: {businessCentreFile.FileSpaceId} tccPath: {tccPath}");
        memStream = await fileRepo.GetFile(businessCentreFile.FileSpaceId, tccPath).ConfigureAwait(false);

        if (memStream != null && memStream.CanRead && memStream.Length > 0)
        {
          coordSystemFileContent = new byte[memStream.Length];
          int numBytesToRead = (int)memStream.Length;
          numBytesRead = memStream.Read(coordSystemFileContent, 0, numBytesToRead);
        }
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
            80, $" isAbleToRead: {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 79, e.Message);
      }
      finally
      {
        memStream?.Dispose();
      }

      log.LogInformation(
        $"GetFileContentFromTcc: numBytesRead: {numBytesRead} coordSystemFileContent.Length {coordSystemFileContent?.Length ?? 0}");
      return coordSystemFileContent;
    }

    /// <summary>
    /// get file content from TCC
    ///     note that is is intended to be used for small, DC files only.
    ///     If/when it is needed for large files, 
    ///           e.g. surfaces, you should use a smaller buffer and loop to read.
    /// </summary>
    public static async Task<Stream> GetFileStreamFromTcc(BusinessCenterFile businessCentreFile,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      Stream memStream = null;
      var tccPath = $"{businessCentreFile.Path}/{businessCentreFile.Name}";

      try
      {
        log.LogInformation(
          $"GetFileStreamFromTcc: getBusinessCentreFile fielspaceID: {businessCentreFile.FileSpaceId} tccPath: {tccPath}");
        memStream = await fileRepo.GetFile(businessCentreFile.FileSpaceId, tccPath).ConfigureAwait(false);

        if (memStream == null || !memStream.CanRead || memStream.Length < 1)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
            80, $" isAbleToRead: {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 79, e.Message);
      }

      log.LogInformation($"GetFileStreamFromTcc: Successfully read memstream. bytesReturned: {memStream?.Length ?? 0}");
      return memStream;
    }

    /// <summary>
    /// Writes the importedFile to TCC
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists already
    /// </summary>
    /// <returns></returns>
    public static async Task<FileDescriptor> WriteFileToTCCRepository(
      Stream fileContents, string customerUid, string projectUid,
      string pathAndFileName, bool isSurveyedSurface, DateTime? surveyedUtc, string fileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      var tccPath = $"/{customerUid}/{projectUid}";
      string tccFileName = Path.GetFileName(pathAndFileName);

      if (isSurveyedSurface && surveyedUtc != null) // validation should prevent this
        tccFileName = ImportedFileUtils.IncludeSurveyedUtcInName(tccFileName, surveyedUtc.Value);

      bool ccPutFileResult = false;
      bool folderAlreadyExists = false;
      try
      {
        log.LogInformation(
          $"WriteFileToTCCRepository: fileSpaceId {fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}");
        // check for exists first to avoid an misleading exception in our logs.
        folderAlreadyExists = await fileRepo.FolderExists(fileSpaceId, tccPath).ConfigureAwait(false);
        if (folderAlreadyExists == false)
          await fileRepo.MakeFolder(fileSpaceId, tccPath).ConfigureAwait(false);

        // this does an upsert
        ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, tccFileName, fileContents, fileContents.Length)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.PutFile",
          e.Message);
      }

      if (ccPutFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 53);
      }

      log.LogInformation(
        $"WriteFileToTCCRepository: tccFileName {tccFileName} written to TCC. folderAlreadyExists {folderAlreadyExists}");
      return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
    }

    /// <summary>
    /// Get the FileCreated and Updated UTCs
    ///    and checks that the file exists.
    /// </summary>
    /// <returns></returns>
    public static async Task<DirResult> GetFileInfoFromTccRepository(BusinessCenterFile sourceFile,
      string fileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      DirResult fileEntry = null;

      try
      {
        log.LogInformation(
          $"GetFileInfoFromTccRepository: GetFileList filespaceID: {sourceFile.FileSpaceId} tccPathSource: {sourceFile.Path} sourceFile.Name: {sourceFile.Name}");

        var dirResult = await fileRepo.GetFileList(sourceFile.FileSpaceId, sourceFile.Path, sourceFile.Name);

        log.LogInformation(
          $"GetFileInfoFromTccRepository: GetFileList dirResult: {JsonConvert.SerializeObject(dirResult)}");


        if (dirResult == null || dirResult.entries.Length == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94, "fileRepo.GetFileList");
        }
        else
        {
          fileEntry = dirResult.entries.FirstOrDefault(f =>
            !f.isFolder && (string.Compare(f.entryName, sourceFile.Name, true, CultureInfo.InvariantCulture) == 0));
          if (fileEntry == null)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94,
              "fileRepo.GetFileList");
          }
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94, "fileRepo.GetFileList",
          e.Message);
      }

      return fileEntry;
    }

    /// <summary>
    /// Copies importedFile between filespaces in TCC
    ///     From FilespaceIDBcCustomer\BC Data to FilespaceIdVisionLink\CustomerUid\ProjectUid
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists
    /// </summary>
    /// <returns></returns>
    public static async Task<FileDescriptor> CopyFileWithinTccRepository(ImportedFileTbc sourceFile,
      string customerUid, string projectUid, string dstFileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      var srcTccPathAndFile = $"{sourceFile.Path}/{sourceFile.Name}";
      var destTccPath = $"/{customerUid}/{projectUid}";

      string tccDestinationFileName = sourceFile.Name;
      if (sourceFile.ImportedFileTypeId == ImportedFileType.SurveyedSurface)
        tccDestinationFileName =
          ImportedFileUtils.IncludeSurveyedUtcInName(tccDestinationFileName, sourceFile.SurfaceFile.SurveyedUtc);

      var destTccPathAndFile = $"/{customerUid}/{projectUid}/{tccDestinationFileName}";
      var tccCopyFileResult = false;

      try
      {
        // The filename already contains the surveyUtc where appropriate
        log.LogInformation(
          $"CopyFileWithinTccRepository: srcFileSpaceId: {sourceFile.FileSpaceId} destFileSpaceId {dstFileSpaceId} srcTccPathAndFile {srcTccPathAndFile} destTccPathAndFile {destTccPathAndFile}");

        // check for exists first to avoid an misleading exception in our logs.
        var folderAlreadyExists = await fileRepo.FolderExists(dstFileSpaceId, destTccPath).ConfigureAwait(false);
        if (folderAlreadyExists == false)
          await fileRepo.MakeFolder(dstFileSpaceId, destTccPath).ConfigureAwait(false);

        // this creates folder if it doesn't exist, and upserts file if it does
        tccCopyFileResult = await fileRepo
          .CopyFile(sourceFile.FileSpaceId, dstFileSpaceId, srcTccPathAndFile, destTccPathAndFile)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 92, "fileRepo.PutFile",
          e.Message);
      }

      if (tccCopyFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 92);
      }

      var fileDescriptorTarget =
        FileDescriptor.CreateFileDescriptor(dstFileSpaceId, destTccPath, tccDestinationFileName);
      log.LogInformation(
        $"CopyFileWithinTccRepository: fileDescriptorTarget {JsonConvert.SerializeObject(fileDescriptorTarget)}");
      return fileDescriptorTarget;
    }

    /// <summary>
    /// Deletes the importedFile from TCC
    /// </summary>
    /// <returns></returns>
    public static async Task<ImportedFileInternalResult> DeleteFileFromTCCRepository(FileDescriptor fileDescriptor, Guid projectUid, Guid importedFileUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo, IProjectRepository projectRepo)
    {
      log.LogInformation($"DeleteFileFromTCCRepository: fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)}");

      bool ccFileExistsResult = false;
      try
      {
        ccFileExistsResult = await fileRepo
          .FileExists(fileDescriptor.filespaceId, fileDescriptor.path + '/' + fileDescriptor.fileName)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(e, $"DeleteFileFromTCCRepository FileExists failed with exception. importedFileUid:{importedFileUid}");
        return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 57, "fileRepo.FileExists", e.Message);
      }

      if (ccFileExistsResult == true)
      {
        bool ccDeleteFileResult = false;
        try
        {
          ccDeleteFileResult = await fileRepo.DeleteFile(fileDescriptor.filespaceId,
              fileDescriptor.path + '/' + fileDescriptor.fileName)
            .ConfigureAwait(false);
        }
        catch (Exception e)
        {
          log.LogError(e, $"DeleteFileFromTCCRepository DeleteFile failed with exception. importedFileUid:{importedFileUid}.");
          return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 57, "fileRepo.DeleteFile", e.Message);
        }

        if (ccDeleteFileResult == false)
        {
          log.LogError(
            $"DeleteFileFromTCCRepository DeleteFile failed to delete importedFileUid:{importedFileUid}.");
          return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 54);
        }
      }
      else
      {
        log.LogInformation(
          $"DeleteFileFromTCCRepository File doesn't exist in TCC {JsonConvert.SerializeObject(fileDescriptor)}");
      }
      return null;
    }
  }
}

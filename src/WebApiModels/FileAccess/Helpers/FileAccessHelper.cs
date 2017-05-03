using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCCFileAccess;
using VSS.Raptor.Service.WebApiModels.FileAccess.Models;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.FileAccess.Helpers
{
  public static class FileAccessHelper
  {
    /// <summary>
    /// Downloads a file from a TCC filespace to a specified local filepath.
    /// </summary>
    /// <param name="fileAccess">File repository.</param>
    /// <param name="request">An instance of FileAccessRequest class.</param>
    public static void DownloadFile(IFileRepository fileAccess, FileAccessRequest request)
    {
      var downloadFileResult = fileAccess.GetFile(request.file.filespaceId, fullName(request.file.path, request.file.fileName)).Result;

      if ((downloadFileResult != null) && (downloadFileResult.Length > 0))
      {
        var fileStream = File.Create(request.localPath);
        downloadFileResult.Seek(0, SeekOrigin.Begin);
        downloadFileResult.CopyTo(fileStream);
        fileStream.Close();
      }
    }

    public static void DownloadFile(IFileRepository fileAccess, FileDescriptor file, Stream stream)
    {
      var downloadFileResult = fileAccess.GetFile(file.filespaceId, fullName(file.path, file.fileName)).Result;

      if ((downloadFileResult != null) && (downloadFileResult.Length > 0))
      {
        downloadFileResult.Seek(0, SeekOrigin.Begin);
        downloadFileResult.CopyTo(stream);
      }
    }
    
    /// <summary>
    /// Returns a full name of the file.
    /// </summary>
    /// <param name="path">File's path.</param>
    /// <param name="fileName">File's name.</param>
    /// <returns></returns>
    private static string fullName(string path, string fileName)
    {
      string fullName = string.IsNullOrEmpty(fileName) ? path : Path.Combine(path, fileName);
      return fullName.Replace(Path.DirectorySeparatorChar, '/');
    }

  }
}

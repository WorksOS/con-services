using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;

namespace VSS.Hydrology.WebApi.Common.Helpers
{
  public class HydroRequestHelper
  {
    /// <summary> zip up images </summary>
    public static string ZipImages(string localProjectPath, string localZipPath, string responseFileName, ILogger log, IServiceExceptionHandler serviceExceptionHandler)
    {
      var finalZippedFile = Path.Combine(localProjectPath, responseFileName);
      if (File.Exists(finalZippedFile))
        File.Delete(finalZippedFile);
      try
      {
        ZipFile.CreateFromDirectory(localZipPath, finalZippedFile, CompressionLevel.Optimal, false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 7, e.Message);
      }

      return finalZippedFile;
    }

  }
}

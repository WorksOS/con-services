using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;
using VSS.MasterData.Models.Handlers;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Hydrology.WebApi.Common.Helpers
{
  public class HydroRequestHelperLatestGround
  {
    private static readonly HydroErrorCodesProvider HydroErrorCodesProvider = new HydroErrorCodesProvider();

    public static async Task<Stream> GetCurrentGround3Dp(HydroRequest request, 
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> customHeaders,
      IRaptorProxy raptorProxy)
    {
      var currentGroundTTMStream = new MemoryStream();
      Stream currentGroundTTMStreamCompressed = null;

      try
      {
        currentGroundTTMStreamCompressed =
          await raptorProxy.GetExportSurface(request.ProjectUid, request.FileName, request.FilterUid, customHeaders, true);
      }
      catch (ServiceException se)
      {
        log.LogError(se, $"{nameof(GetCurrentGround3Dp)}: RaptorServices failed with service exception.");
        //rethrow this to surface it
        throw;
      }
      catch (Exception e)
      {
        log.LogError(e, $"{nameof(GetCurrentGround3Dp)}: {HydroErrorCodesProvider.FirstNameWithOffset(23)}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 23, nameof(GetCurrentGround3Dp), e.Message);
      }

      if (currentGroundTTMStreamCompressed == null)
      {
        log.LogError($"{nameof(GetCurrentGround3Dp)}: {HydroErrorCodesProvider.FirstNameWithOffset(24)}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 24, nameof(GetCurrentGround3Dp));
        return null;
      }

      //3dpm returns a zipped ttm we need to extract it.
      try
      {
        using (var temp = new ZipArchive(currentGroundTTMStreamCompressed))
        {
          if (temp.Entries.Count > 1 ||
              !(temp.Entries[0].FullName.EndsWith("ttm", StringComparison.InvariantCultureIgnoreCase)))
          {
            log.LogError($"{nameof(GetCurrentGround3Dp)}: {HydroErrorCodesProvider.FirstNameWithOffset(26)}");
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 26,
              nameof(GetCurrentGround3Dp));
          }

          await temp.Entries[0].Open().CopyToAsync(currentGroundTTMStream);
        }
      }
      catch (ServiceException se)
      {
        throw se;
      }
      catch (Exception e)
      {
        log.LogError(e,$"{nameof(GetCurrentGround3Dp)}: {HydroErrorCodesProvider.FirstNameWithOffset(27)}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 27,
          nameof(GetCurrentGround3Dp), innerException: e);
      }

      //GracefulWebRequest should throw an exception if the web api call fails but just in case...
      if (currentGroundTTMStream.Length == 0)
      {
        currentGroundTTMStream?.Close();
        currentGroundTTMStreamCompressed.Close();
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 22);
      }

      log.LogInformation($"{nameof(GetCurrentGround3Dp)} ttmFile length: {currentGroundTTMStream.Length} ");
      return currentGroundTTMStream;
    }

    public static Stream GetCurrentGroundTest(ILogger log)
    {
      var ttmLocalPathAndFileName = "..\\..\\test\\UnitTests\\TestData\\Large Sites Road - Trimble Road.ttm";
      log.LogDebug($"{Environment.CurrentDirectory}");
      if (!File.Exists(ttmLocalPathAndFileName))
        throw new InvalidOperationException("unable to find temp ttm");

      var fileStream = new FileStream(ttmLocalPathAndFileName, FileMode.Open);
      var stream = new MemoryStream();
      fileStream.CopyTo(stream);
      fileStream.Close();
      return stream;
    }
  }
}

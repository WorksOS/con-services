using System;
using System.Net;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// internal result to avoid multiple calls to Undelete for Raptor path
  /// this mimics a ServiceException so the calling method can generate the throw
  /// </summary>
  public class ImportedFileInternalResult
  {
    public HttpStatusCode StatusCode;
    public int ErrorNumber;
    public string ResultCode = null;
    public string ErrorMessage1 = null;
    public string ErrorMessage2 = null;
    public Exception InnerException = null;

    private ImportedFileInternalResult()
    { }

    public static ImportedFileInternalResult CreateImportedFileInternalResult
     ( HttpStatusCode statusCode, int errorNumber, string resultCode = null,
      string errorMessage1 = null, string errorMessage2 = null, Exception innerException = null)
    {
      return new ImportedFileInternalResult
      {
        StatusCode = statusCode,
        ErrorNumber = errorNumber,
        ResultCode = resultCode,
        ErrorMessage1 = errorMessage1,
        ErrorMessage2 = errorMessage2,
        InnerException = innerException
      };
    }
  }
}

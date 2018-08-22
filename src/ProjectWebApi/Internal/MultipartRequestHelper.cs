using System;
using System.IO;
using Microsoft.Net.Http.Headers;
using VSS.MasterData.Project.WebAPI.Controllers.Filters;

namespace VSS.MasterData.Project.WebAPI.Internal
{
  /// <summary>
  /// Instead of buffering the file in its entirety we will stream the file upload.
  /// This does introduce challenges as we can no longer use the built in model binding of ASP.net core (see
  /// the <see cref="DisableFormValueModelBindingAttribute"/> action filter).
  /// </summary>
  public static class MultipartRequestHelper
  {
    // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec says 70 characters is a reasonable limit.
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
      var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).ToString();
      if (string.IsNullOrWhiteSpace(boundary))
      {
        throw new InvalidDataException("Missing content-type boundary.");
      }

      if (boundary.Length > lengthLimit)
      {
        throw new InvalidDataException(
          $"Multipart boundary length limit {lengthLimit} exceeded.");
      }

      return boundary;
    }

    public static bool IsMultipartContentType(string contentType)
    {
      return !string.IsNullOrEmpty(contentType)
             && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
      // Content-Disposition: form-data; name="key";
      return contentDisposition != null
             && contentDisposition.DispositionType.Equals("form-data")
             && string.IsNullOrEmpty(contentDisposition.FileName.ToString())
             && string.IsNullOrEmpty(contentDisposition.FileNameStar.ToString());
    }

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
      // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
      return contentDisposition != null
             && contentDisposition.DispositionType.Equals("form-data")
             && (!string.IsNullOrEmpty(contentDisposition.FileName.ToString())
                 || !string.IsNullOrEmpty(contentDisposition.FileNameStar.ToString()));
    }
  }
}

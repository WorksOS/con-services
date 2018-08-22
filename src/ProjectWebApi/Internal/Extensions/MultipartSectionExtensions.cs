using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace VSS.MasterData.Project.WebAPI.Internal.Extensions
{
  /// <summary>
  /// <see cref="MultipartSection"/> extension methods.
  /// </summary>
  public static class MultipartSectionExtensions
  {
    /// <summary>
    /// Returns the <see cref="Encoding"/> type for a given <see cref="MultipartSection"/> object. 
    /// </summary>
    public static Encoding GetEncoding(this MultipartSection section)
    {
      var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

      // UTF-7 is insecure and should not be honored. UTF-8 will succeed in most cases.
      if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
      {
        return Encoding.UTF8;
      }

      return mediaType.Encoding;
    }
  }
}

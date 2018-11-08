using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace VSS.MasterData.Project.WebAPI.Internal.Extensions
{
  /// <summary>
  /// Extension methods for HttpRequest.
  /// </summary>
  public static class HttpRequestExtensions
  {
    /// <summary>
    /// Handle multi-part form data file streams.
    /// </summary>
    public static async Task<string> StreamFile(this HttpRequest request, string filename, ILogger log)
    {
      var formAccumulator = new KeyValueAccumulator();
      string targetFilePath = null;
      var defaultFormOptions = new FormOptions();

      // Boundary is required by the MultipartReader, but we'll auto generate one in the helper, it 
      // doesn't need to be provided in the request header.
      var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), defaultFormOptions.MultipartBoundaryLengthLimit);

      var reader = new MultipartReader(boundary, request.Body);
      var section = await reader.ReadNextSectionAsync();

      log.LogDebug("Processing file stream...");

      while (section != null)
      {
        var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
        var dispositionParameters = contentDisposition.Parameters.Aggregate("", (current, parameter) => $"{current},{parameter}");
        log.LogDebug($"Processing section '{contentDisposition.DispositionType}:{dispositionParameters}'");

        if (hasContentDispositionHeader)
        {
          if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
          {
            targetFilePath = Path.Combine(Path.GetTempPath(), filename);
            log.LogDebug($"Writing stream to '{targetFilePath}'");

            using (var targetStream = File.Create(targetFilePath))
            {
              await section.Body.CopyToAsync(targetStream);
            }
          }
          else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
          {
            // Do not limit the key name length here because the multipart headers length limit is already in effect.
            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
            var encoding = section.GetEncoding();

            using (var streamReader = new StreamReader(
              section.Body,
              encoding,
              detectEncodingFromByteOrderMarks: true,
              bufferSize: 1024,
              leaveOpen: true))
            {
              // The value length limit is enforced by MultipartBodyLengthLimit
              var value = await streamReader.ReadToEndAsync();
              if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
              {
                value = string.Empty;
              }

              formAccumulator.Append(key.ToString(), value);

              if (formAccumulator.ValueCount > defaultFormOptions.ValueCountLimit)
              {
                throw new InvalidDataException($"Form key count limit {defaultFormOptions.ValueCountLimit} exceeded.");
              }
            }
          }
        }

        // Drains any remaining section body that has not been consumed and reads the headers for the next section.
        section = await reader.ReadNextSectionAsync();
      }

      log.LogDebug("Completed processing request body.");

      return targetFilePath;
    }
  }
}

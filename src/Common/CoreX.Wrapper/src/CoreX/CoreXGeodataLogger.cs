using System.IO;
using Microsoft.Extensions.Logging;

namespace CoreX.Wrapper
{
  internal static class CoreXGeodataLogger
  {
    private static readonly object _lock = new object();
    private static bool _isLogged;

    /// <summary>
    /// Dump fileinfo for all geodetic database files found for a given path.
    /// </summary>
    /// <remarks>
    /// This class is designed to work in an environment with multiple CoreX.Wrapper instances being created.
    /// It's behaviour is designed to parse and log the available geodata files only once, regardless of how
    /// many CoreX.Wrapper instance are started.
    /// </remarks>
    public static void DumpGeodataFiles(ILogger log, string geodeticDatabasePath)
    {
      if (_isLogged)
      {
        return;
      }

      lock (_lock)
      {
        log.LogDebug($"CoreX {nameof(DumpGeodataFiles)}: Dumping all available geodetic database files:");

        var files = Directory.GetFiles(geodeticDatabasePath);

        if (files.Length == 0)
        {
          log.LogWarning($"CoreX {nameof(DumpGeodataFiles)}: No files found.");
          return;
        }

        foreach (var file in Directory.GetFiles(geodeticDatabasePath))
        {
          var fileInfo = new FileInfo(file);
          var fileDetails = $"{fileInfo.FullName}, Hash: {fileInfo.GetHashCode()}, Last updated UTC: {fileInfo.LastWriteTimeUtc}";

          log.LogDebug($"{fileDetails}");
        }

        _isLogged = true;
      }
    }
  }
}

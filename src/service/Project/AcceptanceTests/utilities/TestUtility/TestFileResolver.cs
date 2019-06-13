using System;
using System.Collections.Generic;
using System.IO;

namespace TestUtility
{
  public class TestFileResolver
  {
    private static readonly Dictionary<string, string> _uniqueTestFilenames = new Dictionary<string, string>();

    public static string File(string filename) => HashFilename(filename);

    private static string HashFilename(string filename)
    {
      lock (_uniqueTestFilenames)
      {
        // Create a unique file identifier to be used in place of the actual filename.
        var fileIdentifier = Guid.NewGuid() + filename.Substring(filename.Length - 4);
        _uniqueTestFilenames.Add(fileIdentifier, filename);

        return fileIdentifier;
      }
    }

    public static string GetFullPath(string fileKey) => Path.Combine("FileImportFiles", fileKey);
    public static string GetRealFilePath(string fileKey) => Path.Combine("FileImportFiles", GetRealFilename(fileKey));

    public static string GetRealFilename(string fileKey)
    {
      lock (_uniqueTestFilenames)
      {
        _uniqueTestFilenames.TryGetValue(fileKey, out var filenameActual);
        
        return string.IsNullOrEmpty(filenameActual)
          ? fileKey
          : filenameActual;
      }
    }
  }
}

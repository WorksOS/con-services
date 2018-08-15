using System;
using System.IO;
using System.Reflection;

namespace RaptorSvcAcceptTestsCommon.Utils
{
  public static class DirectoryAgent
  {
    /// <summary>
    /// Recursively look upwards for a specific folder and return the full path if exists.
    /// Returns null if not found.
    /// </summary>
    public static string TraverseParentDirectories(string folderName)
    {
      var filePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
      var parts = filePath.Split(Path.DirectorySeparatorChar);

      for (var i = 1; i < parts.Length; i++)
      {
        var testPath = Path.Combine(filePath, folderName);

        if (Directory.Exists(testPath))
        {
          return testPath;
        }

        filePath = Directory.GetParent(filePath).FullName;
      }

      return null;
    }
  }
}

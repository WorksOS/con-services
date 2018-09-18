using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebApiTests.Utilities
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
        var dir = parts.Take(parts.Length - i)
          .Aggregate((p1, p2) => $"{p1}{Path.DirectorySeparatorChar}{p2}");

        var testPath = Path.Combine(dir, folderName);
        if (Directory.Exists(testPath))
        {
          return testPath;
        }
      }

      return null;
    }
  }
}

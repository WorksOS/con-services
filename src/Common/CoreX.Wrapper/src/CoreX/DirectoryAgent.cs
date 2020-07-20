using System;
using System.IO;
using System.Reflection;

namespace CoreX.Wrapper
{
  public static class DirectoryAgent
  {
    /// <summary>
    /// Recursively look upwards for a specific folder and return the full path if exists.
    /// Returns null if not found.
    /// </summary>
    public static string TraverseParentDirectories(string folderName)
    {
     var filePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath.Replace('/', Path.DirectorySeparatorChar);
     //var filePath = Directory.GetCurrentDirectory();
    // var filePath = Directory.GetParent(Assembly.GetExecutingAssembly().CodeBase).FullName;
      var parts = filePath.Split(Path.DirectorySeparatorChar);

      for (var i = 1; i < parts.Length; i++)
      {
        var testPath = Path.Combine(filePath, folderName);
        Console.WriteLine($"*** TESTPATH *** {testPath}");

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

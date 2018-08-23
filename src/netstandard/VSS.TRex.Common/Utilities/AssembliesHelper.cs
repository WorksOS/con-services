using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace VSS.TRex.Common.Utilities
{
  public static class AssembliesHelper
  {
    public static void LoadAllAssembliesForExecutingContext()
    {
      List<Assembly> allAssemblies = new List<Assembly>();
      string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      foreach (string dll in Directory.GetFiles(path, "*.dll"))
        try
        {
          allAssemblies.Add(Assembly.LoadFile(dll));
        }
        catch
        {
          // Ignore these exceptions
        }
    }
  }
}

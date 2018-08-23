using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VSS.TRex.Common.Utilities
{
  public static class AssembliesHelper
  {
    public static void LoadAllAssembliesForExecutingContext()
    {
      // Find already loaded assemblies
      Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

      List<Assembly> allAssemblies = new List<Assembly>();
      string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      foreach (string dll in Directory.GetFiles(path, "*.dll"))
        try
        {
          // Only load the assembly if not already present
          if (!asms.Any(x => x.Location.Equals(dll)))
            allAssemblies.Add(Assembly.LoadFile(dll));
        }
        catch
        {
          // Ignore these exceptions
        }
    }
  }
}

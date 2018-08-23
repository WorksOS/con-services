using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Common.Utilities
{
  public static class AssembliesHelper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public static void LoadAllAssembliesForExecutingContext()
    {
      // Find already loaded assemblies
      Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

      List<Assembly> allAssemblies = new List<Assembly>();
      string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      Log.LogInformation($"Assemblies currently loaded");
      foreach (Assembly asm in asms)
        Log.LogInformation($"{asm.FullName}");

      Log.LogInformation($"Loading additional assmemblies from {path}");

      foreach (string dll in Directory.GetFiles(path, "*.dll"))
        try
        {
          // Only load the assembly if not already present
          if (!asms.Any(x => x.Location.Equals(dll)))
          {
            Log.LogInformation($"Loading TRex assembly {dll}");

            allAssemblies.Add(Assembly.LoadFile(dll));
          }
        }
        catch (Exception ex)
        {
          Log.LogError($"Exception raised while loading assembly {dll}\n{ex}");
        }

      Log.LogInformation($"Assemblies present after loading");
      foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        Log.LogInformation($"{asm.FullName}");
    }
  }
}

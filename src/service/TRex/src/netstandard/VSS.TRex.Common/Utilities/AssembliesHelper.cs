using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Common.Utilities
{
  [AttributeUsage(AttributeTargets.Assembly)]
  public class ForceAssemblyReference : Attribute
  {
    public ForceAssemblyReference(Type forcedType)
    {
      //not sure if these two lines are required since 
      //the type is passed to constructor as parameter, 
      //thus effectively being used
      Action<Type> noop = _ => { };
      noop(forcedType);
    }
  }

  /// <summary>
  /// Iterates through all assemblies present in the same folder as the executing context and loads them if they are not
  /// already present in the loaded assemblies in the current application domain.
  /// WARNING: This does not work well in DotNetCore contexts as dependent assemblies loaded as a result of loading
  /// primary assembles referenced in the project are not contained in the assemblies list returned by GetAssemblies.
  /// DotNetCore contexts will need to directly reference members of the required assemblies to ensure they are included
  /// by the compiler/linker.
  /// </summary>
  public static class AssembliesHelper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public static void LoadAllAssembliesForExecutingContext()
    {
      // Find already loaded assemblies
      var asms = AppDomain.CurrentDomain.GetAssemblies();

      var allAssemblies = new List<Assembly>();
      var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      Log.LogInformation("");
      Log.LogInformation("Assemblies currently loaded");
      Log.LogInformation( "==========================");

      foreach (var asm in asms)
        Log.LogInformation($"{asm.FullName}");

      Log.LogInformation("");
      Log.LogInformation($"Loading additional assemblies from {path}");
      Log.LogInformation("====================================");

      foreach (var dll in Directory.GetFiles(path, "*.dll"))
      {
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
      }

      Log.LogInformation("");
      Log.LogInformation("Assemblies present after loading");
      Log.LogInformation("================================");

      foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        Log.LogInformation($"{asm.FullName}");
    }
  }
}

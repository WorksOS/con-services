using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSS.TRex.Common.Utilities
{
  /// <summary>
  /// Provides handy reflection based helpers for discovering types
  /// </summary>
  public static class TypesHelper
  {
    /// <summary>
    /// Find all of the types that represent classes within the current assembly derived from another class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<Type> FindAllDerivedTypesInAllLoadedAssemblies<T>(string prefix)
    {
      var types = AppDomain.CurrentDomain
          .GetAssemblies()
          .Where(x => x.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
          .SelectMany(FindAllDerivedTypes<T>).ToList();

      return types;
    }

    /// <summary>
    /// Find all of the types that represent classes within the current assembly derived from another class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<Type> FindAllDerivedTypes<T>() => FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));

    /// <summary>
    /// Find all of the types that represent classes within a provided assembly derived from another class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
    {
      try
      {
        var derivedType = typeof(T);
        return assembly
          .GetTypes()
          .Where(t =>
            t != derivedType &&
            derivedType.IsAssignableFrom(t)
          ).ToList();
      }
      catch (System.Reflection.ReflectionTypeLoadException)
      {
        // Ignore the exception and return an empty list
        return new List<Type>();
      }
    }
  }
}

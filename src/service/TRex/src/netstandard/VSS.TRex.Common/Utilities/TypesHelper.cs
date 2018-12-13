using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSS.TRex.Common.Utilities
{
    /// <summary>
    /// Provides handy rflection based haelpers for discovering types
    /// </summary>
    public static class TypesHelper
    {
        /// <summary>
        /// Find all of the types that represent classes within the current assembly derived from another class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        /// <summary>
        /// Find all of the types that represent classes within a provided assembly derived from another class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();
        }
    }
}

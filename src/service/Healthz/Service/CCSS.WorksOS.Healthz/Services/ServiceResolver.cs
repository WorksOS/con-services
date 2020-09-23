using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;

namespace CCSS.WorksOS.Healthz.Services
{
  public static class ServiceResolver
  {
    private static readonly List<string> _serviceIdentifierIgnorelist = new List<string>
    {
      ServiceNameConstants.ASSETMGMT3D_SERVICE
    };

    /// <summary>
    /// Return a list of service identifiers from the <see cref="ServiceNameConstants"/> pool.
    /// </summary>
    public static List<string> GetKnownServiceIdentifiers()
    {
      var fieldInfoObjs = GetClassConstants(typeof(ServiceNameConstants)).ToList();
      var result = new List<string>();

      foreach (var (constant, constantValue) in from constant in fieldInfoObjs
                                                let constantValue = constant.GetRawConstantValue() as string
                                                select (constant, constantValue))
      {
        // Dedupe and remove those from the ignore list.
        if (_serviceIdentifierIgnorelist.Contains(constantValue) || result.Contains(constantValue))
        {
          continue;
        }

        result.Add(constant.GetRawConstantValue() as string);
      }

      return result;
    }

    private static IEnumerable<FieldInfo> GetClassConstants(Type type) =>
      type.GetFields(
        BindingFlags.Public |
        BindingFlags.Static |
        BindingFlags.FlattenHierarchy)
      .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
  }
}

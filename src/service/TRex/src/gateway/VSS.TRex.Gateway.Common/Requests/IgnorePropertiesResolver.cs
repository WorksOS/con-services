using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VSS.TRex.Gateway.Common.Requests
{
  public class IgnorePropertiesResolver : DefaultContractResolver
  {
    private IEnumerable<string> _propsToIgnore;

    public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
    {
      _propsToIgnore = propNamesToIgnore;
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
      var property = base.CreateProperty(member, memberSerialization);
      property.ShouldSerialize = (x) => { return !_propsToIgnore.Contains(property.PropertyName); };

      return property;
    }
  }
}

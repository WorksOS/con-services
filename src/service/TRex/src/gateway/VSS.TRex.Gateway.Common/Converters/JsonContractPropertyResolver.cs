using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VSS.TRex.Gateway.Common.Converters
{
  public class JsonContractPropertyResolver : DefaultContractResolver
  {

    private readonly string[] props;

    /// <inheritdoc />
    public JsonContractPropertyResolver(params string[] prop)
    {
      props = prop;
    }

    /// <inheritdoc />
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
      return base.CreateProperties(type, memberSerialization)
          .Where(p => !props.Contains(p.PropertyName)).ToList();
    }
  }
}


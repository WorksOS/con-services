using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Used by <see cref="T:Newtonsoft.Json.JsonSerializer" /> to serialize a <see cref="T:Newtonsoft.Json.Serialization.JsonContract" />
  /// type with n number of ignored properties.
  /// </summary>
  /// <usage>
  /// e.g. new JsonSerializerSettings { ContractResolver = new DynamicContractResolver("Prop1", "Prop2", ...) }
  /// </usage>
  /// <remarks>
  /// See https://www.newtonsoft.com/json/help/html/CustomContractResolver.htm for more information.
  /// </remarks>
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

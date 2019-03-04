using Apache.Ignite.Core.Cluster;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric.NodeFilters
{
  /// <summary>
  /// Defines a node filter that filters nodes based on a defined role attribute
  /// </summary>
  public abstract class RoleBasedNodeFilter : IClusterNodeFilter, IBinarizable, IFromToBinary
  {
    private const int VERSION_NUMBER = 1;

    /// <summary>
    /// The node role
    /// </summary>
    public string Role { get; private set; } = "";

    /// <summary>
    ///  Default no-arg constructor
    /// </summary>
    public RoleBasedNodeFilter()
    {
    }

    /// <summary>
    /// Constructor accepting the name of the role to filter nodes with
    /// </summary>
    /// <param name="role"></param>
    public RoleBasedNodeFilter(string role) : this()
    {
      Role = role;
    }

    /// <summary>
    /// Implementation of the filter that is provided with node references to determine if they match the filter
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual bool Invoke(IClusterNode node)
    {
      // No implementation in base class, reject the node
      return node.Attributes.Contains(new KeyValuePair<string, object>($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True"));
    }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(Role);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Role = reader.ReadString();
    }
  }
}

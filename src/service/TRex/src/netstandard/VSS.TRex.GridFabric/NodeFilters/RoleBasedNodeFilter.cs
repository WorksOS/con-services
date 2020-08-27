using Apache.Ignite.Core.Cluster;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric.NodeFilters
{
  /// <summary>
  /// Defines a node filter that filters nodes based on a defined role attribute
  /// </summary>
  public abstract class RoleBasedNodeFilter : VersionCheckedBinarizableSerializationBase, IClusterNodeFilter
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
    public RoleBasedNodeFilter(string role) : this()
    {
      Role = role;
    }

    /// <summary>
    /// Implementation of the filter that is provided with node references to determine if they match the filter
    /// </summary>
    public virtual bool Invoke(IClusterNode node)
    {
      // No implementation in base class, reject the node
      return node.Attributes.Contains(new KeyValuePair<string, object>($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True"));
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(Role);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Role = reader.ReadString();
      }
    }
  }
}

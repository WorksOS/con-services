using System;
using Apache.Ignite.Core.Cluster;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric.NodeFilters
{
  /// <summary>
  /// Defines a node filter that filters nodes based on a defined role attribute
  /// </summary>
  public abstract class RoleBasedNodeFilter : IClusterNodeFilter, IBinarizable, IFromToBinary, IEquatable<RoleBasedNodeFilter>
  {
    private const int versionNumber = 1;

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
      writer.WriteByte(versionNumber);
      writer.WriteString(Role);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      var version = reader.ReadByte();

      if (version != versionNumber)
        throw new TRexSerializationVersionException(versionNumber, version);

      Role = reader.ReadString();
    }

    public bool Equals(RoleBasedNodeFilter other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(Role, other.Role);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((RoleBasedNodeFilter) obj);
    }

    public override int GetHashCode()
    {
      return (Role != null ? Role.GetHashCode() : 0);
    }
  }
}

using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.GridFabric.Arguments
{
  /// <summary>
  /// Defines the parameters required for a production data profile request argument on cluster compute nodes
  /// </summary>
  public class ProfileRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument, IEquatable<ProfileRequestArgument_ClusterCompute>
  {
    private const byte kVersionNumber = 1;

    public GridDataType ProfileTypeRequired { get; set; }

    public XYZ[] NEECoords { get; set; } = new XYZ[0];
    
    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

    public DesignDescriptor DesignDescriptor;

    public bool ReturnAllPassesAndLayers { get; set; }

    /// <summary>
    /// Constructs a default profile request argument
    /// </summary>
    public ProfileRequestArgument_ClusterCompute()
    {
    }

    /// <summary>
    /// Creates a new profile request argument initialized with the supplied parameters
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="nEECoords"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    public ProfileRequestArgument_ClusterCompute(GridDataType profileTypeRequired, XYZ[] nEECoords,
      DesignDescriptor designDescriptor, bool returnAllPassesAndLayers)
    {
      ProfileTypeRequired = profileTypeRequired;
      NEECoords = nEECoords;
      DesignDescriptor = designDescriptor;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteByte(kVersionNumber);

      writer.WriteInt((int)ProfileTypeRequired);

      var count = NEECoords?.Length ?? 0;
      writer.WriteInt(count);
      for (int i = 0; i < count; i++)
        NEECoords[i].ToBinary(writer);

      DesignDescriptor.ToBinary(writer);

      writer.WriteBoolean(ReturnAllPassesAndLayers);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      var version = reader.ReadByte();
      if (version != kVersionNumber)
        throw new TRexSerializationVersionException(kVersionNumber, version);

      ProfileTypeRequired = (GridDataType)reader.ReadInt();

      var count = reader.ReadInt();
      NEECoords = new XYZ[count];
      for (int i = 0; i < count; i++)
        NEECoords[i] = NEECoords[i].FromBinary(reader);

      DesignDescriptor.FromBinary(reader);

      ReturnAllPassesAndLayers = reader.ReadBoolean();
    }

    public bool Equals(ProfileRequestArgument_ClusterCompute other)
    {
      return base.Equals(other) && 
             ProfileTypeRequired == other.ProfileTypeRequired && 
             (Equals(NEECoords, other.NEECoords) ||
              (NEECoords != null && other.NEECoords != null && NEECoords.SequenceEqual(other.NEECoords))) &&
             DesignDescriptor.Equals(other.DesignDescriptor) && 
             ReturnAllPassesAndLayers == other.ReturnAllPassesAndLayers;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((ProfileRequestArgument_ClusterCompute) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) ProfileTypeRequired;
        hashCode = (hashCode * 397) ^ (NEECoords != null ? NEECoords.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ DesignDescriptor.GetHashCode();
        hashCode = (hashCode * 397) ^ ReturnAllPassesAndLayers.GetHashCode();
        return hashCode;
      }
    }
  }
}

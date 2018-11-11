using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
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
  public class ProfileRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument, IEquatable<BaseApplicationServiceRequestArgument>
  {
    public GridDataType ProfileTypeRequired { get; set; }

    public XYZ[] NEECoords { get; set; }
    
    // todo LiftBuildSettings: TICLiftBuildSettings;
    // ExternalRequestDescriptor: TASNodeRequestDescriptor;

    public DesignDescriptor DesignDescriptor;

    public bool ReturnAllPassesAndLayers { get; set; } = false;

    /// <summary>
    /// Constgructs a default profile request argumnent
    /// </summary>
    public ProfileRequestArgument_ClusterCompute()
    {
    }

    /// <summary>
    /// Creates a new profile reuest argument initialised with the supplied parameters
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
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteInt((int)ProfileTypeRequired);

      writer.WriteBoolean(NEECoords != null);

      if (NEECoords != null)
      {
        writer.WriteInt(NEECoords.Length);
        foreach (var xyz in NEECoords)
          xyz.ToBinary(writer);
      }

      DesignDescriptor.ToBinary(writer);

      writer.WriteBoolean(ReturnAllPassesAndLayers);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      ProfileTypeRequired = (GridDataType)reader.ReadInt();

      if (reader.ReadBoolean())
      {
        var count = reader.ReadInt();
        NEECoords = new XYZ[count];
        foreach (var xyz in NEECoords)
          xyz.FromBinary(reader);
      }

      DesignDescriptor.FromBinary(reader);

      ReturnAllPassesAndLayers = reader.ReadBoolean();
    }

    protected bool Equals(ProfileRequestArgument_ClusterCompute other)
    {
      return base.Equals(other) && 
             ProfileTypeRequired == other.ProfileTypeRequired && 
             (Equals(NEECoords, other.NEECoords) ||
              (NEECoords != null && other.NEECoords != null && NEECoords.SequenceEqual(other.NEECoords))) &&
             DesignDescriptor.Equals(other.DesignDescriptor) && 
             ReturnAllPassesAndLayers == other.ReturnAllPassesAndLayers;
    }

    public new bool Equals(BaseApplicationServiceRequestArgument other)
    {
      return Equals(other as ProfileRequestArgument_ClusterCompute);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
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

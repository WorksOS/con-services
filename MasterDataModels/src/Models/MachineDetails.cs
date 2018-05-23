using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.FIlters;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// A representation of a machine in a Raptor project
  /// </summary>
  public class MachineDetails : IEquatable<MachineDetails>
  {
    private const int MAX_MACHINE_NAME = 256;

    /// <summary>
    /// The ID of the machine/asset. This is the unique identifier, used by Raptor.
    /// </summary>
    /// <remarks>
    /// Previously JohnDoe machines were identified by having MAX_LONG as the assetID. There are loss of precision errors when browsers
    /// deserialize MAX_LONG so to avoid this we return the id as a string.
    /// </remarks>
    [JsonConverter(typeof(FormatLongAsStringConverter))]
    [JsonProperty(PropertyName = "assetID", Required = Required.Always)]
    public long AssetId { get; protected set; }

    /// <summary>
    /// The textual name of the machine. This is the human readable machine name from the machine control display, and written in tagfiles.
    /// </summary>
    [MaxLength(MAX_MACHINE_NAME)]
    [NameValidation]
    [JsonProperty(PropertyName = "machineName", Required = Required.Always)]
    public string MachineName { get; protected set; }

    /// <summary>
    /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
    /// </summary>
    [JsonProperty(PropertyName = "isJohnDoe", Required = Required.Always)]
    public bool IsJohnDoe { get; protected set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    protected MachineDetails()
    { }

    /// <summary>
    /// Create instance of MachineDetails
    /// </summary>
    public static MachineDetails Create(long assetId, string machineName, bool isJohnDoe)
    {
      return new MachineDetails
      {
        AssetId = assetId,
        MachineName = machineName,
        IsJohnDoe = isJohnDoe
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //Nothing else to validate
    }

    public bool Equals(MachineDetails other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return AssetId == other.AssetId && string.Equals(MachineName, other.MachineName) && IsJohnDoe == other.IsJohnDoe;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((MachineDetails)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = AssetId.GetHashCode();
        hashCode = (hashCode * 397) ^ (MachineName != null ? MachineName.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ IsJohnDoe.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(MachineDetails left, MachineDetails right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(MachineDetails left, MachineDetails right)
    {
      return !Equals(left, right);
    }
  }
}

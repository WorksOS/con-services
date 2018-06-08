using System;

namespace RaptorSvcAcceptTestsCommon.Models
{
  /// <summary>
  /// A representation of a machine in a Raptor project.
  /// </summary>
  public class MachineDetails : IEquatable<MachineDetails>
  {
    /// <summary>
    /// The ID of the machine/asset
    /// </summary>
    public long AssetId { get; set; }

    /// <summary>
    /// The textual name of the machine
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// Is the machine not represented by a telematics device (PLxxx, SNMxxx etc)
    /// </summary>
    public bool IsJohnDoe { get; set; }

    public bool Equals(MachineDetails other)
    {
      if (other == null)
        return false;

      return AssetId == other.AssetId &&
             MachineName == other.MachineName &&
             IsJohnDoe == other.IsJohnDoe;
    }

    public static bool operator ==(MachineDetails a, MachineDetails b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(MachineDetails a, MachineDetails b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is MachineDetails details && this == details;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}

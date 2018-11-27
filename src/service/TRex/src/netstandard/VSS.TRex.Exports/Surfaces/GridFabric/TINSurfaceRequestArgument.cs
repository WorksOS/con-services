using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the Patches request
  /// </summary>
  public class TINSurfaceRequestArgument : BaseApplicationServiceRequestArgument, IEquatable<TINSurfaceRequestArgument>
  {
    /// <summary>
    /// The tolerance to use (in meters) when decimating the elevation surface into a TIN
    /// </summary>
    public double Tolerance { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDouble(Tolerance);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      Tolerance = reader.ReadDouble();
    }

    public bool Equals(TINSurfaceRequestArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && Tolerance.Equals(other.Tolerance);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((TINSurfaceRequestArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (base.GetHashCode() * 397) ^ Tolerance.GetHashCode();
      }
    }
  }
}

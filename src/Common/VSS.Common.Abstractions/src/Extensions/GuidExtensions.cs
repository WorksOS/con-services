using System;

namespace VSS.Common.Abstractions.Extensions
{
  public static class GuidExtensions
  {
    /// <summary>
    /// This method takes a GUID and returns a long that is derived from that GUID
    /// NOTE: You are taking 128 bits of data, and returning 64 bits of data
    /// This should only be used when constraining by other properties
    /// The use for this, is to covert a guid to a constant legacy Raptor ID for backwards compability.
    /// (GetHashCode can't be used, as it can change the result based on the system it's run on)
    /// </summary>
    public static long ToLegacyId(this Guid g)
    {
      var result = BitConverter.ToInt64(g.ToByteArray(), 8);
      return result;
    }
  }
}

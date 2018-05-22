using System;

namespace VSS.TRex.Common
{
  public static class Consts
  {
    public const double NullReal = 1E308;

    /// <summary>
    /// IEEE single/float null value
    /// </summary>
    public const float NullSingle = Single.MaxValue;

    /// <summary>
    /// IEEE single/float null value
    /// </summary>
    public const float NullFloat = Single.MaxValue;

    /// <summary>
    /// IEEE double null value
    /// </summary>
    public const double NullDouble = NullReal; //Double.MaxValue;

    /// <summary>
    /// Value representing a null height encoded as an IEEE single
    /// </summary>
    public const float NullHeight = -3.4E38f;

		public const ushort NullMachineSpeed = UInt16.MaxValue;

		/// <summary>
		/// Null ID for a design reference descriptor ID
		/// </summary>
		public const int kNoDesignNameID = 0;

    /// <summary>
    /// ID representing any design ID in a filter
    /// </summary>
    public const int kAllDesignsNameID = -1;

    /// <summary>
    /// Largest GPS accuracy error value
    /// </summary>
    public const ushort kMaxGPSAccuracyErrorLimit = 0x3FFF;
  }
}

using System;

namespace VSS.TRex.Common
{
  public static class Consts
  {
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
		public const double NullDouble = Double.MaxValue;

    /// <summary>
    /// Value representing a null height encoded as an IEEE single
    /// </summary>
    public const float NullHeight = NullSingle;

		// Value representing a null machine speed encoded as an IEEE ushort
		public const ushort NullMachineSpeed = UInt16.MaxValue;

		/// <summary>
		/// Value representing a minimum material temperature encoded as an IEEE ushort
		/// </summary>
		public const ushort MinMaterialTempValue = 0;
	  /// <summary>
	  /// Value representing a maximum material temperature encoded as an IEEE ushort
	  /// </summary>
	  public const ushort MaxMaterialTempValue = 4095;
  	// Value representing a null material temperature encoded as an IEEE ushort
		public const ushort NullMaterialTemperature = MaxMaterialTempValue + 1;

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

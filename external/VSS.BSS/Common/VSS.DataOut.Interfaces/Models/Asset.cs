using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models
{
  [Serializable]
  public class Asset
  {
    /// <remarks>This is the VisionLink Asset unique identifier (NH_OP..AssetUid), NOT the Asset Name.</remarks>
    public Guid AssetUid { get; set; }

    /// <remarks>Optional, must be supplied with SerialNumber.</remarks>
    /// <remarks>A string with max length of 16 alphanumeric chars</remarks>
    public string MakeCode { get; set; }

    /// <remarks>Optional, must be supplied with MakeCode.</remarks>
    /// <remarks>A string with max length of 128 alphanumeric chars</remarks>
  public string SerialNumber { get; set; }

    /// <remarks>Optional, if omitted the MakeCode and SerialNumber tags must be supplied.</remarks>
    /// <remarks>a string with max length of 100 alphanumeric chars</remarks>
    public string VIN { get; set; }
  }
}

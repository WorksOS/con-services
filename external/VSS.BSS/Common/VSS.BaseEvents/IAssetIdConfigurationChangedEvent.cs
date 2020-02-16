using System;

namespace VSS.BaseEvents
{
  public interface IAssetIdConfigurationChangedEvent : IEndpointDestinedEvent
  {
    Guid AssetUid { get; set; }
    long AssetID { get; set; }
    string MakeCode { get; set; }
    string SerialNumber { get; set; }
    string VIN { get; set; }
    DateTime TimestampUtc { get; set; }

    string AssetAlias { get; set; }
  }
}

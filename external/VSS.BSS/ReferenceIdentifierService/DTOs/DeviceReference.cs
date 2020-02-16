using System;

namespace VSS.Nighthawk.ReferenceIdentifierService.DTOs
{
  public class DeviceReference
  {
    public long Id { get; set; }
    public long StoreId { get; set; }
    public Guid UID { get; set; }
    public string Alias { get; set; }
    public string Value { get; set; }
  }
}

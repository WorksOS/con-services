using System;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests
{
  public class IdentifierDefinition
  {
    public long StoreId { get; set; }
    public string Alias { get; set; }
    public string Value { get; set; }
    public Guid UID { get; set; }
  }
}

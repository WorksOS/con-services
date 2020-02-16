using System;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs
{
  public class LookupResponse<T>
  {
    public T Data { get; set; }
    public Exception Exception { get; set; }
  }
}

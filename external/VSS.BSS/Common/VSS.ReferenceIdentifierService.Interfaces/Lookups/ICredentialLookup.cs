using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups
{
  public interface ICredentialLookup
  {
    Credentials Get(string url);
  }
}

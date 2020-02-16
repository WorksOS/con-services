using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Nighthawk.ReferenceIdentifierService.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface ICredentialManager
  {
    Credentials RetrieveByUrl(string identifierDefinition);
  }
}

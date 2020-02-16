using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class CredentialManager : ICredentialManager
  {
    private readonly IStorage _storage;
    public CredentialManager(IStorage storage)
    {
      _storage = storage;
    }

    public Credentials RetrieveByUrl(string url)
    {
      return _storage.FindCredentialsForUrl(url);
    }
  }
}

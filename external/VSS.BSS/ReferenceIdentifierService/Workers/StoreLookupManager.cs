using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class StoreLookupManager : IStoreLookupManager
  {
    private readonly IStorage _storage;

    public StoreLookupManager(IStorage storage)
    {
      _storage = storage;
    }

    public long FindStoreByCustomerId(long customerId)
    {
      return _storage.FindStoreByCustomerId(customerId);
    }
  }
}

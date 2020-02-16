using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;

namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public class OemLookupManager: IOemLookupManager
  {
    private readonly IStorage _storage;

    public OemLookupManager(IStorage storage)
    {
      _storage = storage;
    }

    public int FindOemIdentifierByCustomerId(long customerId)
    {
      return _storage.FindOemIdentifierByCustomerId(customerId);
    }
  }
}

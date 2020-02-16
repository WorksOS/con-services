
namespace VSS.Nighthawk.ReferenceIdentifierService.Workers
{
  public interface IOemLookupManager
  {
    int FindOemIdentifierByCustomerId(long customerId);
  }
}


namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups
{
  public interface IOemLookup
  {
    int FindOemIdentifierByCustomerId(long customerId);
  }
}

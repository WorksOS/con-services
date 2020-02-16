using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers
{
  public interface IQueryHelper
  {
    T QueryServiceToCreate<T, K>(string svcUri, K queryObject);
    T QueryServiceToUpdate<T, K>(string svcUri, K queryObject);
    T QueryServiceToRetrieve<T, K>(string svcUri, K queryObject);
    T GetByQuery<T>(string svcUri, string query, string action = ControllerConstants.QueryServiceGetActionName);
  }
}

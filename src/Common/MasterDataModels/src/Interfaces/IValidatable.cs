using Microsoft.AspNetCore.Mvc;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;

namespace VSS.MasterData.Models.Interfaces
{
  /// <summary>
  /// Defines if a domain object can have business validation
  /// </summary>
  public interface IValidatable
  {
    /// <summary>
    /// Validate domain object. If validation is not successful throw <see cref="ServiceException" />
    /// </summary>
    void Validate([FromServices] IServiceExceptionHandler serviceExceptionHandler);
  }
}
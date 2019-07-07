using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  public class ServiceExceptionTests
  {
    [Fact]
    public void CanCreateServiceException()
    {
      var exception = new ServiceException(HttpStatusCode.Accepted, new ContractExecutionResult());
      Assert.NotNull(exception);
      Assert.Equal(HttpStatusCode.Accepted, exception.Code);
    }

    [Fact]
    public void CanOverrideStatusCode()
    {
      var exception = new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));
      exception.OverrideBadRequest(HttpStatusCode.NoContent);
      Assert.Equal(HttpStatusCode.NoContent, exception.Code);
    }

    [Fact]
    public void CanNotOverrideStatusCode()
    {
      var exception = new ServiceException(HttpStatusCode.Ambiguous,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));
      exception.OverrideBadRequest(HttpStatusCode.NoContent);
      Assert.NotEqual(HttpStatusCode.NoContent, exception.Code);
    }
  }
}

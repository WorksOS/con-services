using System.Net;
using FluentAssertions;
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

      exception.Should().NotBeNull();
      exception.Code.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public void CanOverrideStatusCode()
    {
      var exception = new ServiceException(
        HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      exception.OverrideBadRequest(HttpStatusCode.NoContent);
      exception.Code.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void CanNotOverrideStatusCode()
    {
      var exception = new ServiceException(
        HttpStatusCode.Ambiguous,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      exception.OverrideBadRequest(HttpStatusCode.NoContent);
      exception.Code.Should().NotBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public void Should_serialize_using_lowerCamelCase()
    {
      var json = new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult()).GetContent;

      json.Should().Be("{\"code\":0,\"message\":\"success\"}");
    }
  }
}

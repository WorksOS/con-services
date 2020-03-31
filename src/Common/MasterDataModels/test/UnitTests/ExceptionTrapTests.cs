using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace VSS.MasterData.Models.UnitTests
{
  //TODO: Remove when all services use WebApi package
  [Obsolete]
  public class ExceptionTrapTests : BaseTest
  {
    public ExceptionTrapTests()
    {
      base.InitTest();
    }

    [Fact]
    public void CanCreateExceptionTrap()
    {
      Assert.NotNull(new ExceptionsTrap(null, null));
    }

    [Fact]
    public async Task CanExecuteExceptionTrap()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);
      var trap = new ExceptionsTrap(MockRequestDelegate, null);
      await trap.Invoke(mockHttpContext.Object);
      //nothing to assert here - make sure that no exceptions are thrown
    }

/* TODO: DefaultHttpResponse is not available in .Net Core 3.0+. Need to understand how to replace it use in the two tests below      

[Fact]
public async Task ThrowsNonAuthenticatedException()
{
var mockHttpContext = new Mock<HttpContext>();
var defaultResponse = new DefaultHttpResponse(new DefaultHttpContext());
mockHttpContext.SetupGet(mc => mc.Response).Returns(defaultResponse);
Task MockRequestDelegate(HttpContext context) => throw new AuthenticationException();
var trap = new ExceptionsTrap(MockRequestDelegate, null);
await trap.Invoke(mockHttpContext.Object);
Assert.Equal((int)HttpStatusCode.Unauthorized, defaultResponse.StatusCode);
}

[Fact]
public async Task ThrowsServiceException()
{
var mockHttpContext = new Mock<HttpContext>();
var defaultResponse = new DefaultHttpResponse(new DefaultHttpContext());
mockHttpContext.SetupGet(mc => mc.Response).Returns(defaultResponse);
Task MockRequestDelegate(HttpContext context) => throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult());
var trap = new ExceptionsTrap(MockRequestDelegate, ServiceProvider.GetRequiredService<ILogger<ExceptionsTrap>>());
await trap.Invoke(mockHttpContext.Object);
Assert.Equal((int)HttpStatusCode.BadRequest, defaultResponse.StatusCode);
}
*/

}
}

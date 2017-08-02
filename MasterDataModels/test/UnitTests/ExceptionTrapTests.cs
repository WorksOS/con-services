using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Log4Net.Extensions;

namespace VSS.MasterData.Models.Tests
{
  [TestClass]
  public class ExceptionTrapTests
  {

    public IServiceProvider ServiceProvider;


    /// <summary>
    /// Initializes the test.
    /// </summary>
    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateExceptionTrap()
    {
      Assert.IsNotNull(new ExceptionsTrap(null,null));
    }

    [TestMethod]
    public async Task CanExecuteExceptionTrap()
    {
      var mockHttpContext = new Mock<HttpContext>();
      RequestDelegate mockRequestDelegate = context => Task.FromResult(mockHttpContext.Object);
      var trap = new ExceptionsTrap(mockRequestDelegate,null);
      await trap.Invoke(mockHttpContext.Object);
      //nothing to assert here - make sure that no exceptions are thrown
    }

    [TestMethod]
    public async Task ThrowsNonAuthenticatedException()
    {
      var mockHttpContext = new Mock<HttpContext>();
      var defaultResponse = new DefaultHttpResponse(new DefaultHttpContext());
      mockHttpContext.SetupGet(mc => mc.Response).Returns(defaultResponse);
      RequestDelegate mockRequestDelegate = context => throw new AuthenticationException();
      var trap = new ExceptionsTrap(mockRequestDelegate, null);
      await trap.Invoke(mockHttpContext.Object);
      Assert.AreEqual((int)HttpStatusCode.Unauthorized,defaultResponse.StatusCode);
    }

    [TestMethod]
    public async Task ThrowsServiceException()
    {
      var mockHttpContext = new Mock<HttpContext>();
      var defaultResponse = new DefaultHttpResponse(new DefaultHttpContext());
      mockHttpContext.SetupGet(mc => mc.Response).Returns(defaultResponse);
      RequestDelegate mockRequestDelegate = context => throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult());
      var trap = new ExceptionsTrap(mockRequestDelegate, ServiceProvider.GetRequiredService<ILogger<ExceptionsTrap>>());
      await trap.Invoke(mockHttpContext.Object);
      Assert.AreEqual((int)HttpStatusCode.BadRequest, defaultResponse.StatusCode);
    }

  }
}
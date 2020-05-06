using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;
using VSS.Productivity3D.TagFileGateway.Common.Proxy;
using VSS.Serilog.Extensions;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
    [TestClass]
    public class TagFileGatewayTests
    {
        private static IServiceProvider _serviceProvider;
        private static ILoggerFactory _logger;
        private static Dictionary<string, string> _customHeaders;

        private static Mock<IConfigurationStore> _mockStore = new Mock<IConfigurationStore>();

        private static CompactionTagFileRequest request =
          new CompactionTagFileRequest
          {
              ProjectId = 554,
              ProjectUid = Guid.NewGuid(),
              FileName = "Machine Name--whatever--161230235959.tag",
              Data = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 },
              OrgId = string.Empty
          };


        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _serviceProvider = new ServiceCollection()
              .AddLogging()
              .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
              .AddSingleton<IConfigurationStore>(_mockStore.Object)
              .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
              .AddTransient<ITagFileForwarder, TagFileForwarderProxy>() // Class under test
#if RAPTOR
        .AddTransient<IErrorCodesProvider, RaptorResult>()
#endif
        .BuildServiceProvider();

            _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
            _customHeaders = new Dictionary<string, string>();
        }

        [TestMethod]
        public void TestDirectSuccess()
        {
            // Setup a single tag file send
            var forwarder = new Mock<TagFileForwarderProxy>(new Mock<IWebRequest>().Object, _mockStore.Object, _logger, new Mock<IDataCache>().Object, new Mock<IServiceResolution>().Object)
            {
                CallBase = true
            };
            forwarder.Setup(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
                "/tagfiles/direct",
                It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))))
              .Returns(Task.FromResult(new ContractExecutionResult(0)));

            var result = forwarder.Object.SendTagFileDirect(request, _customHeaders).Result;

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            // Validate we only tried to send the file once - with the correct values
            forwarder.Verify(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
              "/tagfiles/direct",
              It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))), Times.Once);
        }

        [TestMethod]
        public void TestNonDirectSuccess()
        {
            // Setup a single tag file send
            var forwarder = new Mock<TagFileForwarderProxy>(new Mock<IWebRequest>().Object, _mockStore.Object, _logger, new Mock<IDataCache>().Object, new Mock<IServiceResolution>().Object)
            {
                CallBase = true
            };
            forwarder.Setup(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
                "/tagfiles",
                It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))))
              .Returns(Task.FromResult(new ContractExecutionResult(0)));

            var result = forwarder.Object.SendTagFileNonDirect(request, _customHeaders).Result;

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            // Validate we only tried to send the file once - with the correct values
            forwarder.Verify(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
              "/tagfiles",
              It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))), Times.Once);
        }

        [TestMethod]
        public void TestNonZeroFailure()
        {
            var callCount = 0;
            // Setup a non zero result for first try, then success on second try
            var forwarder = new Mock<TagFileForwarderProxy>(new Mock<IWebRequest>().Object, _mockStore.Object, _logger, new Mock<IDataCache>().Object, new Mock<IServiceResolution>().Object)
            {
                CallBase = true
            };
            forwarder.Setup(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
                "/tagfiles",
                It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))))
              .Callback(() => callCount++)
              .Returns(() =>
              {
                  if (callCount == 1)
                      return Task.FromResult(new ContractExecutionResult(1));
                  else
                      return Task.FromResult(new ContractExecutionResult(0));
              });

            // Test
            var result = forwarder.Object.SendTagFileNonDirect(request, _customHeaders).Result;

            // Validate - should be ok, but 2 calls
            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            forwarder.Verify(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
              "/tagfiles",
              It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))), Times.Exactly(2));
        }

        [TestMethod]
        public void TestNullFailure()
        {
            var callCount = 0;
            // Setup a non zero result for first try, then success on second try
            var forwarder = new Mock<TagFileForwarderProxy>(new Mock<IWebRequest>().Object, _mockStore.Object, _logger, new Mock<IDataCache>().Object, new Mock<IServiceResolution>().Object)
            {
                CallBase = true
            };
            forwarder.Setup(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
                "/tagfiles",
                It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))))
              .Callback(() => callCount++)
              .Returns(() =>
              {
                  if (callCount == 1)
                      return Task.FromResult<ContractExecutionResult>(null);
                  else
                      return Task.FromResult(new ContractExecutionResult(0));
              });

            var result = forwarder.Object.SendTagFileNonDirect(request, _customHeaders).Result;

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            forwarder.Verify(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
              "/tagfiles",
              It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))), Times.Exactly(2));
        }

        [TestMethod]
        public void TestExceptionFailure()
        {
            var callCount = 0;
            // Setup a non zero result for first try, then success on second try
            var forwarder = new Mock<TagFileForwarderProxy>(new Mock<IWebRequest>().Object, _mockStore.Object, _logger, new Mock<IDataCache>().Object, new Mock<IServiceResolution>().Object)
            {
                CallBase = true
            };
            forwarder.Setup(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
                "/tagfiles",
                It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))))
              .Callback(() => callCount++)
              .Returns(() =>
              {
                  if (callCount == 1)
                      throw new Exception();
                  else
                      return Task.FromResult(new ContractExecutionResult(0));
              });

            var result = forwarder.Object.SendTagFileNonDirect(request, _customHeaders).Result;

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            forwarder.Verify(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
              "/tagfiles",
              It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))), Times.Exactly(2));
        }

        [TestMethod]
        public void TestMulitpleFailures()
        {
            // Setup a non zero result for first try, then success on second try
            var forwarder = new Mock<TagFileForwarderProxy>(new Mock<IWebRequest>().Object, _mockStore.Object, _logger, new Mock<IDataCache>().Object, new Mock<IServiceResolution>().Object)
            {
                CallBase = true
            };
            forwarder.Setup(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
                "/tagfiles",
                It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))))
              .Returns(() =>
              {
                  throw new Exception("mock-message");
              });

            var result = forwarder.Object.SendTagFileNonDirect(request, _customHeaders).Result;

            result.Should().NotBeNull();
            result.Code.Should().Be(1);
            result.Message.Should().Be("mock-message");

            forwarder.Verify(m => m.SendSingleTagFile(It.Is<CompactionTagFileRequest>(r => r == request),
              "/tagfiles",
              It.Is<IDictionary<string, string>>(d => Equals(d, _customHeaders))), Times.Exactly(3));
        }
    }
}

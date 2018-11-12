using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.ConnectedSite.Gateway.Executors;
using VSS.TRex.ConnectedSite.Gateway.WebApi;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Models;
using VSS.TRex.ConnectedSite.Gateway.WebApi.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;
using Xunit;

namespace VSS.Trex.ConnectedSiteGateway.Tests
{
  public class ConnectedSiteMessageSubmissionExecutorTests
  {
    byte[] GoodTagfile;
    ServiceCollection services = new ServiceCollection();
    Mock<IConfigurationStore> mockConfigStore = new Mock<IConfigurationStore>();
    Mock<IServiceExceptionHandler> mockServiceExceptionHandler = new Mock<IServiceExceptionHandler>();

    public ConnectedSiteMessageSubmissionExecutorTests()
    {
      GoodTagfile = Convert.FromBase64String("EAABFAAACKxNCLki90wHt14NzS4KXQaxbhbQBuFtALUL43k2ufC6NAT5TUrNCEn5FATqHuwKK2UYNApQIE1YuFkJo0BP/trpo2GTkUBOfzPAdNRSg0BsNrQDJqGRaRaBZxZkZY1kJjAGIGEGAF8AIHAARABlAHMAaQBnAG4AIABPAEcATgAAU0qT4mO6OOQ/Uh7oXPhwDwDAUd/ydkAGn4FAgoKEgoGCgoKCgoKCkoGCgoOEg4KCkYKCgYKChISFg4WBgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoKCkoKBgoKCk4KCgoKClIGCgoKCgoKCgYKRgoKCgoKCgYKCgoKCgoKCkoKBgoKCgoKCgoKRgYKCgoKCgoGCgoKCgoKBkoKCgoKCgoKBgoKCgoKCgpGCgYKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCkoKCgYKCgoKCgoKTgoKCgYKUgoKCgoKVgoGCgoKUgoKCgoKRgYKCgoKCgoKCgYKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoGCgoKCgoKCgoGCkoKCgoKCkYKBgoKCkoKCgoKCk4GCgoKClIKCgoKBgoKCgoKRgoKCgYKSgoKCgoKCgoGCgpGCgoKCgoKBgoKCkoKCgoKBgoKCgoKRgoKCgYKShIOCgpOBgoKCgpSCgoKCgYKCgoKClYKCgYKCk4KCgoKCkYGCgoKCkoKCgoKBgoKCgoKTgoKCgoGSgoKCgoKCgoGCgpOCgoKCgpSCgYKCgpWCgoKCgZaCgoKCgpeCgoKBgpiCgoKCgpmCgr4/27wjrj3bzCWCgYKTgr621MzUrrrUvNiCgoKCkYKCgoGCkoKCgoKCgoKCgYKRgoKCgoKCgoGCgpKCgoKCgoKCgYKCk4KCgoKCkoKBgoKCkYKCgoKCkoKCgYKCkYKCgoKCgoKCgYKCgoKCgoKCgoGCgoKCgoKSgoKCgoKBgoKCgpGCgoKCgpKBgoKCgpOCgoKCgpSBgoKCgr493fxIrjTeLDiVgoKCgoKWgYKCgoKXgoKCgoGYgoKCgoKTgoKCgYKRgoKCgoKCgr7D0hy1rsjR/MGBgoKCgoKCgpKCgYKCgoKCgoKCkYGCgoKCkoKCgoKTgoKCgoKUgYOCgoKBgoKCgpGCgoKCgYKCgoKCgoKCgoGSgoKCgoKCgoKCgZOCgoKCgpSCgoKCgZWDgb483NxBrjzczD2CgoKWgoKCgoKXgYKCgoKYgoKCgoKZgYOBgoKTgoK+yNQMxK7F1By8goKCkYKCgoGCgoKCgoKCgoKCgYKCgoKCkoKCgoKBgoKCgoKRgoKCgoKSgoKBgoKTgoKCgoKUgoKBgoKVgr403Ow/rjnc3EaCgoKCloKCgoKCl4GCgoKCmIKCgoKCk4KCgoKCkYKBvszTfMmuytN8v4KCgoKCgoKCkoKCgYKCk4KCgoKClIKCgoGCgoKCgoKRgoKCgoKBgoKCgoKCgoKCgYKCgoKCgoKCgoGCgoKCgoKCgoKBgoKCgpKCgoKCgoGCgoKCgoKCgoKTgYKCgoKSgoKCgYKTgoKCgoKSgoKBgoKTgoKCgoKSgoKBgoKTgoKCgoKSgYKCgoKCgoKCgpGCgYKCgpKCgoKCgpOCgoGCgpSCgoKCgoKCgYK+NtvMMK4v2/w5gpWCgoKCgpKCgoGCgoKCgoKCkYGCgoKCkoKCgoKCgYKCgoKRgoKCgoKSgoGCgoKCgoKCgpGBgoKCgoKCgoKCgYSCgoKCgoKCkoGCgoKCk4KCgoGClIKCgoKClYKCgoKBloKCgoKCl4KCgr443ZxJrj3dfEKCgpiBg4K+u9QMp6661AyngpOCgoGCgpSCgoKCgpWCgYKCgpaCgoKCgZSCgoKCgpWCgoKCgZaCgoKCgpeCgoKCgZSCgoKCgpWCgoGCgpKCgoKCgpGCgYKCgpKCgoKCgoGCgoKCkYKCgoGCgoKCgoKSgoKBgoKCgoKCgoKCgoKCkYKBgoKCkoKCgoKCgoKBgoKTgoKCgoKSgoGCgoKRgoKCgoKSgYKCgoKCgoKCgpGBgoKCgoKCgoKCkoGCgoKCgoKCgoKRgoGCgoKCgoKCgoKCgYKCgoKCgoKCgoGCgpKCgoKCgoKBgoKCkYKCgoKCgoKBgoKSgoKCgoKCgYKCgpGCgoKCgYKCgoKCgoKCgoGCgoKCgoKCgYKCgoKCgoKCgYKCgoKCgoKBgoKCgoKCgoKBgpKCgoKCgoKCgYKCkYKCgoKCgYOBgoKSgoKCgoKCgYKCgpGCgoKCgpKCgoKBgpOCgoKCgpSCgoKBgpWCgoKCgpaCgoGCgpeCvjTdDECuOdz8Q4KCgoKYgoKBgoKZgoKCgoa1CmFTRLqyCiQE8s305VmI+SQE5VjG5CFSaDQKUNZpesFlHqJAT7Jua2V0g5JATtFdN7MVJ4NAbBagmoKCkIKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoGCgoKCtQC3xsGNkLo0BPKvJRiC6fkUBOUd0/RqhSg0ClANaIzkkAmjQE8ebS2qDJORQE6fyE0SrFODQGw2oYKZgoKCgoKag76S1ayNrpjVfJKCgoKCkoKBgoKCgoKCgoKRgoKCgoKBgoKCgoKCgoKCgYKCgoKCgoKCgYKCgoKCgoKBgpKCgoKCgoKCgYKCk4KCgoKCkoGCgoKCgoKCgoKRgoKBgoKCgoKCgpKBgoKCgoKCgoKBkYKDgYKCgoKCgoKCgYKDgpKCgoKCgoGCgoKRgoKCgoKSgoKCgYKCgoKCgoKCgoKCkYKCgoGCgoKCgoKCgoKBgpKCgoKCgoKCgoGCk4KCgoKCkoKCgYKCk4KCgoKCgoGCgoKRgoKCgoGSgoKCgoKCgoGCgpGCgoKCgoKCgYKCkoKCgoKCgYKCgoKRhEo1MjMwNTkxOTEzAEt0b3JjaABcMjY1MkowNzNTVwBbRDYxIFNBVEQgUEUAWjEyLjMwLTU0MzU1AEMXTUFDSElORVRZUEUANDQVBQTElDQVRJT05fVkVSU0lPTgDFpNQUNISU5FSUQAxbU0VSSUFMAMXERFU0lHTgDXBSRVBPUlRJTkdfV0VFSwB11SRVBPUlRJTkdfVElNRQCV5SQURJT19TRVJJQUwAxKUkFESU9fVFlQRQDEtXRUVLAHTFRJTUUAGFRJTUUAlNTUFQX1NUQVRVUwB19DT05UUk9MX1NUQVRFX1RJTFQAFgQ09OVFJPTF9TVEFURV9MSUZUABYUlOX0FWT0lEX1pPTkUAFiTUlOX0VMRVZfTUFQADY01BUF9SRUNfU1RBVFVTMgAWRNQVBfUkVDX1NUQVRVUwA2VCTEFERV9PTl9HUk9VTkQAFmT05fR1JPVU5EABZ0RJUkVDVElPTgAWhHRUFSABaUFHRQAZVkFMSURfUE9TSVRJT04AFqR1BTX0FDQ1VSQUNZAHa0dQU19NT0RFABbFJJR0hUAOpMRUZUAOtFTEVWQVRJT04ALEVMRVZBVElPTgC05OT1JUSElORwAtTk9SVEhJTkcAtPRUFTVElORwAuRUFTVElORwC1BIRUlHSFQAtRTE9OR0lUVURFALUkxBVElUVURFALU1VUTQA21DT09SRF9TWVNfVFlQRQAW4AA=");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      services.AddLogging().AddSingleton(loggerFactory);
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
    }


    [Fact]
    public async Task L1PositionMessage_Executor_Success()
    {
      var connectedSiteClient = new Mock<IConnectedSiteClient>();
      connectedSiteClient.Setup(client => client.PostMessage(It.Is<IConnectedSiteMessage>(m => m.Route.ToString().Contains("positions/"))))
        .ReturnsAsync(() =>
      {
        return new HttpResponseMessage()
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(string.Empty)
        };
      });

      services.AddSingleton(connectedSiteClient.Object);
      TRex.DI.DIContext.Inject(services.BuildServiceProvider());

      var l1ConnectedSiteRequest = new ConnectedSiteRequest
      {
        TagRequest = new CompactionTagFileRequest
        {
          FileName = "Test File",
          Data = GoodTagfile
        },
        MessageType = ConnectedSiteMessageType.L1PositionMessage
      };

      var result = await RequestExecutorContainer.Build<ConnectedSiteMessageSubmissionExecutor>(
        mockConfigStore.Object,
        TRex.DI.DIContext.Obtain<ILoggerFactory>(),
        mockServiceExceptionHandler.Object
        )
        .ProcessAsync(l1ConnectedSiteRequest) as ConnectedSiteMessageResult;

      result.Should().NotBeNull();
      result.Code.Should().Be(0);
      result.Message.Should().Be(string.Empty);
    }


    [Fact]
    public async Task L2StatusMessage_Executor_Success()
    {
      var connectedSiteClient = new Mock<IConnectedSiteClient>();
      connectedSiteClient.Setup(client => client.PostMessage(It.Is<IConnectedSiteMessage>(m => m.Route.ToString().Contains("status/"))))
        .ReturnsAsync(() =>
        {
          return new HttpResponseMessage()
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(string.Empty)
          };
        });

      services.AddSingleton(connectedSiteClient.Object);
      TRex.DI.DIContext.Inject(services.BuildServiceProvider());

      var l2ConnectedSiteRequest = new ConnectedSiteRequest
      {
        TagRequest = new CompactionTagFileRequest
        {
          FileName = "Test File",
          Data = GoodTagfile
        },
        MessageType = ConnectedSiteMessageType.L2StatusMessage
      };

      var result = await RequestExecutorContainer.Build<ConnectedSiteMessageSubmissionExecutor>(
        mockConfigStore.Object,
        TRex.DI.DIContext.Obtain<ILoggerFactory>(),
        mockServiceExceptionHandler.Object
        )
        .ProcessAsync(l2ConnectedSiteRequest) as ConnectedSiteMessageResult;

      result.Should().NotBeNull();
      result.Code.Should().Be(0);
      result.Message.Should().Be(string.Empty);
    }


    [Fact]
    public async Task StatusMessage_Executor_Bad_Tagfile_Failure()
    {
      var connectedSiteClient = new Mock<IConnectedSiteClient>();
      connectedSiteClient.Setup(client => client.PostMessage(It.IsAny<IConnectedSiteMessage>()))
        .ReturnsAsync(() =>
        {
          return new HttpResponseMessage()
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(string.Empty)
          };
        });

      services.AddSingleton(connectedSiteClient.Object);
      TRex.DI.DIContext.Inject(services.BuildServiceProvider());

      var l2ConnectedSiteRequest = new ConnectedSiteRequest
      {
        TagRequest = new CompactionTagFileRequest
        {
          FileName = "Test File",
          Data = Encoding.ASCII.GetBytes("Bung file")
        },
        MessageType = ConnectedSiteMessageType.L2StatusMessage
      };

      var result = await RequestExecutorContainer.Build<ConnectedSiteMessageSubmissionExecutor>(
        mockConfigStore.Object,
        TRex.DI.DIContext.Obtain<ILoggerFactory>(),
        mockServiceExceptionHandler.Object
        )
        .ProcessAsync(l2ConnectedSiteRequest) as ConnectedSiteMessageResult;

      result.Should().NotBeNull();
      result.Code.Should().Be((int)TRexTagFileResultCode.TRexUnknownException);
      result.Message.Should().Be("TRex unknown result TagFilePreScanExecutor");
    }



    [Fact]
    public async Task StatusMessage_Executor_Send_Failure()
    {
      var connectedSiteClient = new Mock<IConnectedSiteClient>();
      connectedSiteClient.Setup(client => client.PostMessage(It.IsAny<IConnectedSiteMessage>()))
        .ReturnsAsync(() =>
        {
          return new HttpResponseMessage()
          {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent(string.Empty)
          };
        });


      services.AddSingleton(connectedSiteClient.Object);
      TRex.DI.DIContext.Inject(services.BuildServiceProvider());

      var l2ConnectedSiteRequest = new ConnectedSiteRequest
      {
        TagRequest = new CompactionTagFileRequest
        {
          FileName = "Test File",
          Data = Convert.FromBase64String("EAABFAAACKxNCLki90wHt14NzS4KXQaxbhbQBuFtALUL43k2ufC6NAT5TUrNCEn5FATqHuwKK2UYNApQIE1YuFkJo0BP/trpo2GTkUBOfzPAdNRSg0BsNrQDJqGRaRaBZxZkZY1kJjAGIGEGAF8AIHAARABlAHMAaQBnAG4AIABPAEcATgAAU0qT4mO6OOQ/Uh7oXPhwDwDAUd/ydkAGn4FAgoKEgoGCgoKCgoKCkoGCgoOEg4KCkYKCgYKChISFg4WBgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoKCkoKBgoKCk4KCgoKClIGCgoKCgoKCgYKRgoKCgoKCgYKCgoKCgoKCkoKBgoKCgoKCgoKRgYKCgoKCgoGCgoKCgoKBkoKCgoKCgoKBgoKCgoKCgpGCgYKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCkoKCgYKCgoKCgoKTgoKCgYKUgoKCgoKVgoGCgoKUgoKCgoKRgYKCgoKCgoKCgYKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoGCgoKCgoKCgoGCkoKCgoKCkYKBgoKCkoKCgoKCk4GCgoKClIKCgoKBgoKCgoKRgoKCgYKSgoKCgoKCgoGCgpGCgoKCgoKBgoKCkoKCgoKBgoKCgoKRgoKCgYKShIOCgpOBgoKCgpSCgoKCgYKCgoKClYKCgYKCk4KCgoKCkYGCgoKCkoKCgoKBgoKCgoKTgoKCgoGSgoKCgoKCgoGCgpOCgoKCgpSCgYKCgpWCgoKCgZaCgoKCgpeCgoKBgpiCgoKCgpmCgr4/27wjrj3bzCWCgYKTgr621MzUrrrUvNiCgoKCkYKCgoGCkoKCgoKCgoKCgYKRgoKCgoKCgoGCgpKCgoKCgoKCgYKCk4KCgoKCkoKBgoKCkYKCgoKCkoKCgYKCkYKCgoKCgoKCgYKCgoKCgoKCgoGCgoKCgoKSgoKCgoKBgoKCgpGCgoKCgpKBgoKCgpOCgoKCgpSBgoKCgr493fxIrjTeLDiVgoKCgoKWgYKCgoKXgoKCgoGYgoKCgoKTgoKCgYKRgoKCgoKCgr7D0hy1rsjR/MGBgoKCgoKCgpKCgYKCgoKCgoKCkYGCgoKCkoKCgoKTgoKCgoKUgYOCgoKBgoKCgpGCgoKCgYKCgoKCgoKCgoGSgoKCgoKCgoKCgZOCgoKCgpSCgoKCgZWDgb483NxBrjzczD2CgoKWgoKCgoKXgYKCgoKYgoKCgoKZgYOBgoKTgoK+yNQMxK7F1By8goKCkYKCgoGCgoKCgoKCgoKCgYKCgoKCkoKCgoKBgoKCgoKRgoKCgoKSgoKBgoKTgoKCgoKUgoKBgoKVgr403Ow/rjnc3EaCgoKCloKCgoKCl4GCgoKCmIKCgoKCk4KCgoKCkYKBvszTfMmuytN8v4KCgoKCgoKCkoKCgYKCk4KCgoKClIKCgoGCgoKCgoKRgoKCgoKBgoKCgoKCgoKCgYKCgoKCgoKCgoGCgoKCgoKCgoKBgoKCgpKCgoKCgoGCgoKCgoKCgoKTgYKCgoKSgoKCgYKTgoKCgoKSgoKBgoKTgoKCgoKSgoKBgoKTgoKCgoKSgYKCgoKCgoKCgpGCgYKCgpKCgoKCgpOCgoGCgpSCgoKCgoKCgYK+NtvMMK4v2/w5gpWCgoKCgpKCgoGCgoKCgoKCkYGCgoKCkoKCgoKCgYKCgoKRgoKCgoKSgoGCgoKCgoKCgpGBgoKCgoKCgoKCgYSCgoKCgoKCkoGCgoKCk4KCgoGClIKCgoKClYKCgoKBloKCgoKCl4KCgr443ZxJrj3dfEKCgpiBg4K+u9QMp6661AyngpOCgoGCgpSCgoKCgpWCgYKCgpaCgoKCgZSCgoKCgpWCgoKCgZaCgoKCgpeCgoKCgZSCgoKCgpWCgoGCgpKCgoKCgpGCgYKCgpKCgoKCgoGCgoKCkYKCgoGCgoKCgoKSgoKBgoKCgoKCgoKCgoKCkYKBgoKCkoKCgoKCgoKBgoKTgoKCgoKSgoGCgoKRgoKCgoKSgYKCgoKCgoKCgpGBgoKCgoKCgoKCkoGCgoKCgoKCgoKRgoGCgoKCgoKCgoKCgYKCgoKCgoKCgoGCgpKCgoKCgoKBgoKCkYKCgoKCgoKBgoKSgoKCgoKCgYKCgpGCgoKCgYKCgoKCgoKCgoGCgoKCgoKCgYKCgoKCgoKCgYKCgoKCgoKBgoKCgoKCgoKBgpKCgoKCgoKCgYKCkYKCgoKCgYOBgoKSgoKCgoKCgYKCgpGCgoKCgpKCgoKBgpOCgoKCgpSCgoKBgpWCgoKCgpaCgoGCgpeCvjTdDECuOdz8Q4KCgoKYgoKBgoKZgoKCgoa1CmFTRLqyCiQE8s305VmI+SQE5VjG5CFSaDQKUNZpesFlHqJAT7Jua2V0g5JATtFdN7MVJ4NAbBagmoKCkIKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoGCgoKCtQC3xsGNkLo0BPKvJRiC6fkUBOUd0/RqhSg0ClANaIzkkAmjQE8ebS2qDJORQE6fyE0SrFODQGw2oYKZgoKCgoKag76S1ayNrpjVfJKCgoKCkoKBgoKCgoKCgoKRgoKCgoKBgoKCgoKCgoKCgYKCgoKCgoKCgYKCgoKCgoKBgpKCgoKCgoKCgYKCk4KCgoKCkoGCgoKCgoKCgoKRgoKBgoKCgoKCgpKBgoKCgoKCgoKBkYKDgYKCgoKCgoKCgYKDgpKCgoKCgoGCgoKRgoKCgoKSgoKCgYKCgoKCgoKCgoKCkYKCgoGCgoKCgoKCgoKBgpKCgoKCgoKCgoGCk4KCgoKCkoKCgYKCk4KCgoKCgoGCgoKRgoKCgoGSgoKCgoKCgoGCgpGCgoKCgoKCgYKCkoKCgoKCgYKCgoKRhEo1MjMwNTkxOTEzAEt0b3JjaABcMjY1MkowNzNTVwBbRDYxIFNBVEQgUEUAWjEyLjMwLTU0MzU1AEMXTUFDSElORVRZUEUANDQVBQTElDQVRJT05fVkVSU0lPTgDFpNQUNISU5FSUQAxbU0VSSUFMAMXERFU0lHTgDXBSRVBPUlRJTkdfV0VFSwB11SRVBPUlRJTkdfVElNRQCV5SQURJT19TRVJJQUwAxKUkFESU9fVFlQRQDEtXRUVLAHTFRJTUUAGFRJTUUAlNTUFQX1NUQVRVUwB19DT05UUk9MX1NUQVRFX1RJTFQAFgQ09OVFJPTF9TVEFURV9MSUZUABYUlOX0FWT0lEX1pPTkUAFiTUlOX0VMRVZfTUFQADY01BUF9SRUNfU1RBVFVTMgAWRNQVBfUkVDX1NUQVRVUwA2VCTEFERV9PTl9HUk9VTkQAFmT05fR1JPVU5EABZ0RJUkVDVElPTgAWhHRUFSABaUFHRQAZVkFMSURfUE9TSVRJT04AFqR1BTX0FDQ1VSQUNZAHa0dQU19NT0RFABbFJJR0hUAOpMRUZUAOtFTEVWQVRJT04ALEVMRVZBVElPTgC05OT1JUSElORwAtTk9SVEhJTkcAtPRUFTVElORwAuRUFTVElORwC1BIRUlHSFQAtRTE9OR0lUVURFALUkxBVElUVURFALU1VUTQA21DT09SRF9TWVNfVFlQRQAW4AA=")
        },
        MessageType = ConnectedSiteMessageType.L2StatusMessage
      };

      var result = await RequestExecutorContainer.Build<ConnectedSiteMessageSubmissionExecutor>(
        mockConfigStore.Object,
        TRex.DI.DIContext.Obtain<ILoggerFactory>(),
        mockServiceExceptionHandler.Object
        )
        .ProcessAsync(l2ConnectedSiteRequest) as ConnectedSiteMessageResult;

      result.Should().NotBeNull();
      result.Code.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task StatusMessage_Executor_Obtain_Client_Failure()
    {

      TRex.DI.DIContext.Inject(services.BuildServiceProvider());

      var l2ConnectedSiteRequest = new ConnectedSiteRequest
      {
        TagRequest = new CompactionTagFileRequest
        {
          FileName = "Test File",
          Data = Convert.FromBase64String("EAABFAAACKxNCLki90wHt14NzS4KXQaxbhbQBuFtALUL43k2ufC6NAT5TUrNCEn5FATqHuwKK2UYNApQIE1YuFkJo0BP/trpo2GTkUBOfzPAdNRSg0BsNrQDJqGRaRaBZxZkZY1kJjAGIGEGAF8AIHAARABlAHMAaQBnAG4AIABPAEcATgAAU0qT4mO6OOQ/Uh7oXPhwDwDAUd/ydkAGn4FAgoKEgoGCgoKCgoKCkoGCgoOEg4KCkYKCgYKChISFg4WBgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoKCkoKBgoKCk4KCgoKClIGCgoKCgoKCgYKRgoKCgoKCgYKCgoKCgoKCkoKBgoKCgoKCgoKRgYKCgoKCgoGCgoKCgoKBkoKCgoKCgoKBgoKCgoKCgpGCgYKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCkoKCgYKCgoKCgoKTgoKCgYKUgoKCgoKVgoGCgoKUgoKCgoKRgYKCgoKCgoKCgYKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoGCgoKCgoKCgoGCkoKCgoKCkYKBgoKCkoKCgoKCk4GCgoKClIKCgoKBgoKCgoKRgoKCgYKSgoKCgoKCgoGCgpGCgoKCgoKBgoKCkoKCgoKBgoKCgoKRgoKCgYKShIOCgpOBgoKCgpSCgoKCgYKCgoKClYKCgYKCk4KCgoKCkYGCgoKCkoKCgoKBgoKCgoKTgoKCgoGSgoKCgoKCgoGCgpOCgoKCgpSCgYKCgpWCgoKCgZaCgoKCgpeCgoKBgpiCgoKCgpmCgr4/27wjrj3bzCWCgYKTgr621MzUrrrUvNiCgoKCkYKCgoGCkoKCgoKCgoKCgYKRgoKCgoKCgoGCgpKCgoKCgoKCgYKCk4KCgoKCkoKBgoKCkYKCgoKCkoKCgYKCkYKCgoKCgoKCgYKCgoKCgoKCgoGCgoKCgoKSgoKCgoKBgoKCgpGCgoKCgpKBgoKCgpOCgoKCgpSBgoKCgr493fxIrjTeLDiVgoKCgoKWgYKCgoKXgoKCgoGYgoKCgoKTgoKCgYKRgoKCgoKCgr7D0hy1rsjR/MGBgoKCgoKCgpKCgYKCgoKCgoKCkYGCgoKCkoKCgoKTgoKCgoKUgYOCgoKBgoKCgpGCgoKCgYKCgoKCgoKCgoGSgoKCgoKCgoKCgZOCgoKCgpSCgoKCgZWDgb483NxBrjzczD2CgoKWgoKCgoKXgYKCgoKYgoKCgoKZgYOBgoKTgoK+yNQMxK7F1By8goKCkYKCgoGCgoKCgoKCgoKCgYKCgoKCkoKCgoKBgoKCgoKRgoKCgoKSgoKBgoKTgoKCgoKUgoKBgoKVgr403Ow/rjnc3EaCgoKCloKCgoKCl4GCgoKCmIKCgoKCk4KCgoKCkYKBvszTfMmuytN8v4KCgoKCgoKCkoKCgYKCk4KCgoKClIKCgoGCgoKCgoKRgoKCgoKBgoKCgoKCgoKCgYKCgoKCgoKCgoGCgoKCgoKCgoKBgoKCgpKCgoKCgoGCgoKCgoKCgoKTgYKCgoKSgoKCgYKTgoKCgoKSgoKBgoKTgoKCgoKSgoKBgoKTgoKCgoKSgYKCgoKCgoKCgpGCgYKCgpKCgoKCgpOCgoGCgpSCgoKCgoKCgYK+NtvMMK4v2/w5gpWCgoKCgpKCgoGCgoKCgoKCkYGCgoKCkoKCgoKCgYKCgoKRgoKCgoKSgoGCgoKCgoKCgpGBgoKCgoKCgoKCgYSCgoKCgoKCkoGCgoKCk4KCgoGClIKCgoKClYKCgoKBloKCgoKCl4KCgr443ZxJrj3dfEKCgpiBg4K+u9QMp6661AyngpOCgoGCgpSCgoKCgpWCgYKCgpaCgoKCgZSCgoKCgpWCgoKCgZaCgoKCgpeCgoKCgZSCgoKCgpWCgoGCgpKCgoKCgpGCgYKCgpKCgoKCgoGCgoKCkYKCgoGCgoKCgoKSgoKBgoKCgoKCgoKCgoKCkYKBgoKCkoKCgoKCgoKBgoKTgoKCgoKSgoGCgoKRgoKCgoKSgYKCgoKCgoKCgpGBgoKCgoKCgoKCkoGCgoKCgoKCgoKRgoGCgoKCgoKCgoKCgYKCgoKCgoKCgoGCgpKCgoKCgoKBgoKCkYKCgoKCgoKBgoKSgoKCgoKCgYKCgpGCgoKCgYKCgoKCgoKCgoGCgoKCgoKCgYKCgoKCgoKCgYKCgoKCgoKBgoKCgoKCgoKBgpKCgoKCgoKCgYKCkYKCgoKCgYOBgoKSgoKCgoKCgYKCgpGCgoKCgpKCgoKBgpOCgoKCgpSCgoKBgpWCgoKCgpaCgoGCgpeCvjTdDECuOdz8Q4KCgoKYgoKBgoKZgoKCgoa1CmFTRLqyCiQE8s305VmI+SQE5VjG5CFSaDQKUNZpesFlHqJAT7Jua2V0g5JATtFdN7MVJ4NAbBagmoKCkIKCgoKCgoKBgoKCgoKCgoGCgoKCgoKCgoGCgoKCtQC3xsGNkLo0BPKvJRiC6fkUBOUd0/RqhSg0ClANaIzkkAmjQE8ebS2qDJORQE6fyE0SrFODQGw2oYKZgoKCgoKag76S1ayNrpjVfJKCgoKCkoKBgoKCgoKCgoKRgoKCgoKBgoKCgoKCgoKCgYKCgoKCgoKCgYKCgoKCgoKBgpKCgoKCgoKCgYKCk4KCgoKCkoGCgoKCgoKCgoKRgoKBgoKCgoKCgpKBgoKCgoKCgoKBkYKDgYKCgoKCgoKCgYKDgpKCgoKCgoGCgoKRgoKCgoKSgoKCgYKCgoKCgoKCgoKCkYKCgoGCgoKCgoKCgoKBgpKCgoKCgoKCgoGCk4KCgoKCkoKCgYKCk4KCgoKCgoGCgoKRgoKCgoGSgoKCgoKCgoGCgpGCgoKCgoKCgYKCkoKCgoKCgYKCgoKRhEo1MjMwNTkxOTEzAEt0b3JjaABcMjY1MkowNzNTVwBbRDYxIFNBVEQgUEUAWjEyLjMwLTU0MzU1AEMXTUFDSElORVRZUEUANDQVBQTElDQVRJT05fVkVSU0lPTgDFpNQUNISU5FSUQAxbU0VSSUFMAMXERFU0lHTgDXBSRVBPUlRJTkdfV0VFSwB11SRVBPUlRJTkdfVElNRQCV5SQURJT19TRVJJQUwAxKUkFESU9fVFlQRQDEtXRUVLAHTFRJTUUAGFRJTUUAlNTUFQX1NUQVRVUwB19DT05UUk9MX1NUQVRFX1RJTFQAFgQ09OVFJPTF9TVEFURV9MSUZUABYUlOX0FWT0lEX1pPTkUAFiTUlOX0VMRVZfTUFQADY01BUF9SRUNfU1RBVFVTMgAWRNQVBfUkVDX1NUQVRVUwA2VCTEFERV9PTl9HUk9VTkQAFmT05fR1JPVU5EABZ0RJUkVDVElPTgAWhHRUFSABaUFHRQAZVkFMSURfUE9TSVRJT04AFqR1BTX0FDQ1VSQUNZAHa0dQU19NT0RFABbFJJR0hUAOpMRUZUAOtFTEVWQVRJT04ALEVMRVZBVElPTgC05OT1JUSElORwAtTk9SVEhJTkcAtPRUFTVElORwAuRUFTVElORwC1BIRUlHSFQAtRTE9OR0lUVURFALUkxBVElUVURFALU1VUTQA21DT09SRF9TWVNfVFlQRQAW4AA=")
        },
        MessageType = ConnectedSiteMessageType.L2StatusMessage
      };

      var executor = RequestExecutorContainer.Build<ConnectedSiteMessageSubmissionExecutor>(
          mockConfigStore.Object,
          TRex.DI.DIContext.Obtain<ILoggerFactory>(),
          mockServiceExceptionHandler.Object
          );

      Func<Task> result = async () =>
      {
        await executor.ProcessAsync(l2ConnectedSiteRequest);
      };
      result.Should().Throw<ConnectedSiteClientException>()
        .WithMessage("Could not obtain Connected Site Client, have you added it to DI?");
    }
  }
}


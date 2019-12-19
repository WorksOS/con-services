using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class HubMessage
  { }

  public class SimpleHub : Hub
  {
    public override async Task OnConnectedAsync()
    {
      await Welcome();

      await base.OnConnectedAsync();
    }

    public async Task Welcome()
    {
      await Clients.All.SendAsync("welcome", new[] { new HubMessage(), new HubMessage(), new HubMessage() });
    }
  }


  public class SimpleHubTests
  {
    [Fact]
    public async Task SimpleHubTest()
    {
      // arrange
      var mockClients = new Mock<IHubCallerClients>();
      var mockClientProxy = new Mock<IClientProxy>();

      mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);


      var simpleHub = new SimpleHub() { Clients = mockClients.Object };

      // act
      await simpleHub.Welcome();


      // assert
      mockClients.Verify(clients => clients.All, Times.Once);

      mockClientProxy.Verify(
        clientProxy => clientProxy.SendCoreAsync(
          "welcome",
          It.Is<object[]>(o => o != null && o.Length == 1 && ((object[])o[0]).Length == 3),
          default(CancellationToken)),
        Times.Once);
    }
  }
}

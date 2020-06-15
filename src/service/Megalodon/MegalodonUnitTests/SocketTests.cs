using System;
using Microsoft.Extensions.Logging;
using TagFiles;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using FluentAssertions;
using Xunit;

namespace MegalodonUnitTests
{
  public class SocketTests
  {
 //   private ISocketManager _socketManager;
    private IConfigurationStore _config;
  //  private readonly ILogger _log;

    [Fact]
    public void CreateSocketManager()
    {
      var log = new LoggerFactory();
      _config = new TestingConfig(log);
      var sock = new SocketManager(log, _config);
      sock.Should().NotBeNull("Socket Manager is Null");

    }

    [Fact]
    public void TestFluentAssertions()
    {
      string a = "bob";
      a.Should().StartWith("bo").And.EndWith("ob").And.Contain("o").And.HaveLength(3);
    }


    [Fact]
    public void ParseTestFluentAssertions()
    {
      string a = "bob";
      a.Should().StartWith("bo").And.EndWith("ob").And.Contain("o").And.HaveLength(3);
    }


  }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class NotificationMethodTestFixture : IDisposable
  {
    public IServiceProvider ServiceProvider;

    public NotificationMethodTestFixture()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Push.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(loggerFactory)
                       .AddSingleton(new Mock<IConfigurationStore>().Object)
                       .AddSingleton(new Mock<IServiceResolution>().Object)
                       .AddTransient<INotificationHubClient, NotificationHubClient>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public void Dispose()
    { }
  }

  public class NotificationMethodTests : IClassFixture<NotificationMethodTestFixture>
  {
    private readonly NotificationMethodTestFixture TestFixture;

    private const string NOTIFICATION_GUID_KEY = "test_key_do_not_repeat_for_guid";
    private const string NOTIFICATION_OBJ_KEY = "test_key_do_not_repeat_for_obj";
    private const NotificationUidType NOTIFICATION_UID_TYPE = NotificationUidType.Project;

    private static Guid? methodOneGuid;
    private static Guid? methodOneAsyncGuid;

    private static object methodTwoObject;
    private static object methodTwoAsyncObject;

    private void Reset()
    {
      methodOneGuid = null;
      methodOneAsyncGuid = null;
      methodTwoObject = null;
      methodTwoAsyncObject = null;
    }

    public NotificationMethodTests(NotificationMethodTestFixture testFixture)
    {
      Reset();

      TestFixture = testFixture;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_GUID_KEY)]
    public void MethodOne(Guid g)
    {
      methodOneGuid = g;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_GUID_KEY)]
    public Task MethodOneASync(Guid g)
    {
      methodOneAsyncGuid = g;
      return Task.CompletedTask;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_OBJ_KEY)]
    public void MethodTwo(object o)
    {
      methodTwoObject = o;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_OBJ_KEY)]
    public Task MethodTwoAsync(object o)
    {
      methodTwoAsyncObject = o;
      return Task.CompletedTask;
    }

    [Fact]
    public void TestMethodSignatures()
    {
      var notificationHub = TestFixture.ServiceProvider.GetService<INotificationHubClient>() as NotificationHubClient;
      Assert.NotNull(notificationHub);

      const int expectedObj = 12345;
      var expectedGuid = Guid.Parse("D1571C92-FA2C-46F1-8A26-E0809E63B4EA");

      var notificationGuid = new Notification(NOTIFICATION_GUID_KEY,
       expectedGuid,
        NOTIFICATION_UID_TYPE);

      var notificationObj = new Notification(NOTIFICATION_OBJ_KEY,
        expectedObj,
        NOTIFICATION_UID_TYPE);


      Assert.Null(methodOneGuid);
      Assert.Null(methodOneAsyncGuid);
      Assert.Null(methodTwoObject);
      Assert.Null(methodTwoAsyncObject);

      Task.WaitAll(notificationHub.ProcessNotificationAsTasks(notificationGuid).ToArray());
      Task.WaitAll(notificationHub.ProcessNotificationAsTasks(notificationObj).ToArray());

      Assert.Equal(expectedGuid, methodOneGuid);
      Assert.Equal(expectedGuid, methodOneAsyncGuid);
      Assert.Equal(expectedObj, methodTwoObject);
      Assert.Equal(expectedObj, methodTwoAsyncObject);
    }
  }
}

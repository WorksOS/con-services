using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Log4Net.Extensions;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;

namespace VSS.Productivity3D.Push.UnitTests
{
  [TestClass]
  public class NotificationMethodTests
  {
    private const string NOTIFICATION_GUID_KEY = "test_key_do_not_repeat_for_guid";
    private const string NOTIFICATION_OBJ_KEY = "test_key_do_not_repeat_for_obj";
    private const NotificationUidType NOTIFICATION_UID_TYPE = NotificationUidType.Project;

    protected IServiceProvider ServiceProvider;

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

    [TestInitialize]
    public virtual void InitTest()
    {
      Reset();
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug().AddConsole();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<IConfigurationStore>(new Mock<IConfigurationStore>().Object);
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IServiceResolution>(new Mock<IServiceResolution>().Object);
      serviceCollection.AddTransient<INotificationHubClient, NotificationHubClient>(); // This is what we are testing

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_GUID_KEY)]
    public void MethodOne(Guid g)
    {
      methodOneGuid = g;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_GUID_KEY)]
    public  Task MethodOneASync(Guid g)
    {
      methodOneAsyncGuid = g;
      return Task.CompletedTask;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_OBJ_KEY)]
    public  void MethodTwo(object o)
    {
      methodTwoObject = o;
    }

    [Notification(NOTIFICATION_UID_TYPE, NOTIFICATION_OBJ_KEY)]
    public  Task MethodTwoAsync(object o)
    {
      methodTwoAsyncObject = o;
      return Task.CompletedTask;
    }

    [TestMethod]
    public void TestMethodSignatures()
    {
      var notificationHub = ServiceProvider.GetService<INotificationHubClient>() as NotificationHubClient;
      Assert.IsNotNull(notificationHub);

      const int expectedObj = 12345;
      var expectedGuid = Guid.Parse("D1571C92-FA2C-46F1-8A26-E0809E63B4EA");

      var notificationGuid = new Notification(NOTIFICATION_GUID_KEY,
       expectedGuid,
        NOTIFICATION_UID_TYPE);
      
      var notificationObj = new Notification(NOTIFICATION_OBJ_KEY,
        expectedObj,
        NOTIFICATION_UID_TYPE);


      Assert.IsNull(methodOneGuid);
      Assert.IsNull(methodOneAsyncGuid);
      Assert.IsNull(methodTwoObject);
      Assert.IsNull(methodTwoAsyncObject);

      Task.WaitAll(notificationHub.ProcessNotificationAsTasks(notificationGuid).ToArray());
      Task.WaitAll(notificationHub.ProcessNotificationAsTasks(notificationObj).ToArray());
      
      Assert.AreEqual(expectedGuid, methodOneGuid);
      Assert.AreEqual(expectedGuid, methodOneAsyncGuid);
      Assert.AreEqual(expectedObj, methodTwoObject);
      Assert.AreEqual(expectedObj, methodTwoAsyncObject);
   }


  }
}

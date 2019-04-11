using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.Clients.Notifications;

namespace VSS.Productivity3D.Push.UnitTests
{
  [TestClass]
  public class NotificationCacheInvalidationUnitTests
  {
    protected IServiceProvider ServiceProvider;

    protected Mock<IDataCache> mockCache;

    [TestInitialize]
    public virtual void InitTest()
    {
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
      serviceCollection.AddTransient<INotificationHubClient, NotificationHubClient>();

      // This is the main test object
      mockCache = new Mock<IDataCache>();
      serviceCollection.AddSingleton<IDataCache>(mockCache.Object);

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void Test_CacheIsInvalidated()
    {
      var mockCache = new Mock<IDataCache>();

      var tag = new Guid("57579A5B-8368-41E1-9BFA-40341760A81C");

      mockCache.Setup(m => m.RemoveByTag(tag.ToString()));

      // Test the cache invalidation
      var invalidation = new CacheInvalidationService(mockCache.Object);

      invalidation.InvalidateTags(tag);

      mockCache.Verify(m => m.RemoveByTag(tag.ToString()), Times.Once);
    }

    [TestMethod]
    public void Test_CacheInvalidatedOnMessage()
    {
      // Our Invalidation Service is hooked up to the project / customer / user changed notifications
      // It should call remove by tag for each of the events
      // We will test indirectly that this is the case.
      var tag = new Guid("1D6F21DD-8A5B-4ED0-9FBB-7E1B83439BE0");

      mockCache.Setup(m => m.RemoveByTag(tag.ToString()));

      var notificationHub = ServiceProvider.GetService<INotificationHubClient>() as NotificationHubClient;

      Assert.IsNotNull(notificationHub);
      notificationHub.ProcessNotification(new CustomerChangedNotification(tag)); // one

      mockCache.Verify(m => m.RemoveByTag(tag.ToString()), Times.Once);

      notificationHub.ProcessNotification(new ProjectChangedNotification(tag)); // two

      notificationHub.ProcessNotification(new UserChangedNotification(tag)); // three

      // We should have called invalidate cache 3 times (once per notification).
      mockCache.Verify(m => m.RemoveByTag(tag.ToString()), Times.Exactly(3));
    }
  }
}

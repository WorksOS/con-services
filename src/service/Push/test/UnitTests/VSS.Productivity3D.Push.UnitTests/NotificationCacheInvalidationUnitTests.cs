using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using Xunit;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class NotificationCacheInvalidationUnitTests : IClassFixture<NotificationCacheInvalidationFixture>
  {
    private readonly NotificationCacheInvalidationFixture TestFixture;

    public NotificationCacheInvalidationUnitTests(NotificationCacheInvalidationFixture testFixture)
    {
      TestFixture = testFixture;
    }

    [Fact]
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

    [Fact]
    public void Test_CacheInvalidatedOnMessage()
    {
      // Our Invalidation Service is hooked up to the project / customer / user changed notifications
      // It should call remove by tag for each of the events
      // We will test indirectly that this is the case.
      var tag = new Guid("1D6F21DD-8A5B-4ED0-9FBB-7E1B83439BE0");

      TestFixture.MockCache.Setup(m => m.RemoveByTag(tag.ToString()));

      var notificationHub = TestFixture.ServiceProvider.GetService<INotificationHubClient>() as NotificationHubClient;

      Assert.NotNull(notificationHub);
      var tasks = notificationHub.ProcessNotificationAsTasks(new CustomerChangedNotification(tag)); // one
      Task.WaitAll(tasks.ToArray());

      TestFixture.MockCache.Verify(m => m.RemoveByTag(tag.ToString()), Times.Once);

      tasks = notificationHub.ProcessNotificationAsTasks(new ProjectChangedNotification(tag)); // two
      Task.WaitAll(tasks.ToArray());

      tasks = notificationHub.ProcessNotificationAsTasks(new UserChangedNotification(tag)); // three
      Task.WaitAll(tasks.ToArray());

      // We should have called invalidate cache 3 times (once per notification).
      TestFixture.MockCache.Verify(m => m.RemoveByTag(tag.ToString()), Times.Exactly(3));
    }
  }
}

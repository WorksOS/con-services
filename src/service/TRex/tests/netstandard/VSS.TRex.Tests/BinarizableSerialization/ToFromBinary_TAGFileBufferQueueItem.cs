using System;
using VSS.TRex.TAGFiles.Models;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_TAGFileBufferQueueItem
  {
    [Fact]
    public void Test_TAGFileBufferQueueItem_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TAGFileBufferQueueItem>("Empty TAGFileBufferQueueItem not same after round trip serialisation");
    }

    [Fact]
    public void Test_TAGFileBufferQueueItem()
    {
      var testDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1, 1), DateTimeKind.Utc);
      var filter = new TAGFileBufferQueueItem
      {
        Content = new byte[] {1, 10, 25, 100},
        InsertUTC = testDate,
        ProjectID = Guid.NewGuid(),
        AssetID = Guid.NewGuid(),
        IsJohnDoe = true,
        FileName = "fileName"
      };

      var result = SimpleBinarizableInstanceTester.TestClass(filter, "Custom TAGFileBufferQueueItem not same after round trip serialisation");

      Assert.True(result.member.FileName.Equals("fileName") &&
                  result.member.InsertUTC.Equals(testDate), 
        "Post IEquality<T> comparer based comparison failure");
    }
  }
}

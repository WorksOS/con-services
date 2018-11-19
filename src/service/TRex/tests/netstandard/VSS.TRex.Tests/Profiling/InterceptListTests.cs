using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
  public class InterceptListTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_InterceptList_Create()
    {
      InterceptList list = new InterceptList();

      Assert.NotNull(list);
      Assert.True(0 == list.Count, "List not empty after creation");
      Assert.NotNull(list.Items);
    }

    [Fact]
    public void Test_InterceptList_AddPoint1()
    {
      InterceptList list = new InterceptList();

      var newPoint = new InterceptRec(1, 2, 3, 4, 5, 6);
      list.AddPoint(newPoint);

      Assert.True(1 == list.Count, "List count not 1 after addition");
      Assert.True(list.Items[0].Equals(newPoint), "New point and list point not same after addition");
    }

    [Fact]
    public void Test_InterceptList_AddPoint2()
    {
      InterceptList list = new InterceptList();

      var newPoint = new InterceptRec(1, 2, 0, 0, 5, 0);

      list.AddPoint(newPoint);

      Assert.True(1 == list.Count, "List count not 1 after addition");
      Assert.True(list.Items[0].Equals(newPoint), "New point and list point not same after addition");
    }

    [Fact]
    public void Test_InterceptList_MergeInterceptLists_SameItems()
    {
      InterceptList list1 = new InterceptList();
      InterceptList list2 = new InterceptList();

      var newPoint = new InterceptRec(1, 2, 3, 4, 5, 6);

      list1.AddPoint(newPoint);
      list2.AddPoint(newPoint);

      InterceptList mergedList = new InterceptList();
      mergedList.MergeInterceptLists(list1, list2);

      Assert.True(1 == mergedList.Count, $"merged list count != after merge, = {mergedList.Count}");
      Assert.Equal(newPoint, mergedList.Items[0]);
    }

    [Fact]
    public void Test_InterceptList_MergeInterceptLists_UniqueItems_InOrder()
    {
      InterceptList list1 = new InterceptList();
      InterceptList list2 = new InterceptList();

      var newPoint1 = new InterceptRec(1, 2, 3, 4, 5, 6);
      var newPoint2 = new InterceptRec(2, 3, 4, 5, 6, 7);

      list1.AddPoint(newPoint1);
      list2.AddPoint(newPoint2);

      InterceptList mergedList = new InterceptList();
      mergedList.MergeInterceptLists(list1, list2);

      Assert.True(2 == mergedList.Count, $"merged list count != after merge, = {mergedList.Count}");
      Assert.Equal(newPoint1, mergedList.Items[0]);
      Assert.Equal(newPoint2, mergedList.Items[1]);
    }

    [Fact]
    public void Test_InterceptList_MergeInterceptLists_UniqueItems_OutOfOrder()
    {
      InterceptList list1 = new InterceptList();
      InterceptList list2 = new InterceptList();

      var newPoint1 = new InterceptRec(1, 2, 3, 4, 5, 6);
      var newPoint2 = new InterceptRec(2, 3, 4, 5, 6, 7);

      list1.AddPoint(newPoint2);
      list2.AddPoint(newPoint1);

      InterceptList mergedList = new InterceptList();
      mergedList.MergeInterceptLists(list1, list2);

      Assert.True(2 == mergedList.Count, $"merged list count != after merge, = {mergedList.Count}");
      Assert.Equal(newPoint1, mergedList.Items[0]);
      Assert.Equal(newPoint2, mergedList.Items[1]);
    }

    [Fact]
    public void Test_InterceptList_MergeInterceptLists_SameItems_InExactOrder()
    {
      InterceptList list1 = new InterceptList();
      InterceptList list2 = new InterceptList();

      var newPoint1 = new InterceptRec(1.99995f, 2.99995f, 3, 4, 5.99995f, 6);
      var newPoint2 = new InterceptRec(2, 3, 4, 5, 6, 7);

      list1.AddPoint(newPoint1);
      list2.AddPoint(newPoint2);

      InterceptList mergedList = new InterceptList();
      mergedList.MergeInterceptLists(list1, list2);

      Assert.True(1 == mergedList.Count, $"merged list count != after merge, = {mergedList.Count}");
      Assert.Equal(newPoint1, mergedList.Items[0]);
      Assert.Equal(newPoint2, mergedList.Items[0]);
    }

    [Fact]
    public void Test_InterceptList_MergeInterceptLists_SameItems_InExactOutOfOrder()
    {
      InterceptList list1 = new InterceptList();
      InterceptList list2 = new InterceptList();

      var newPoint1 = new InterceptRec(1.99995f, 2.99995f, 3, 4, 5.99995f, 6);
      var newPoint2 = new InterceptRec(2, 3, 4, 5, 6, 7);

      list1.AddPoint(newPoint2);
      list2.AddPoint(newPoint1);

      InterceptList mergedList = new InterceptList();
      mergedList.MergeInterceptLists(list1, list2);

      Assert.True(1 == mergedList.Count, $"merged list count != after merge, = {mergedList.Count}");
      Assert.Equal(newPoint1, mergedList.Items[0]);
      Assert.Equal(newPoint2, mergedList.Items[0]);
    }

    [Fact]
    public void Test_InterceptList_MaximumListLength()
    {
      InterceptList list = new InterceptList();
      for (int i = 0; i < InterceptList.MaxIntercepts + 10; i++)
        list.AddPoint(new InterceptRec(i, i, i, i, i, i));

      Assert.True(InterceptList.MaxIntercepts == list.Count, $"Count not == MaxIntercepts after overfilling list ({InterceptList.MaxIntercepts } vs {list.Count}");
    }

    [Fact]
    public void Test_InterceptList_MergeInterceptLists_MaximumListLength()
    {
      InterceptList list1 = new InterceptList();
      InterceptList list2 = new InterceptList();
      for (int i = 0; i < InterceptList.MaxIntercepts / 2 + 10; i++)
      {
        list1.AddPoint(new InterceptRec(i, i, i, i, i, i));
        list2.AddPoint(new InterceptRec(i + 0.5f, i + 0.5f, i + 0.5f, i + 0.5f, i + 0.5f, i + 0.5f));
      }

      InterceptList mergedList = new InterceptList();
      mergedList.MergeInterceptLists(list1, list2);

      Assert.True(InterceptList.MaxIntercepts == mergedList.Count, $"Count not == MaxIntercepts after overfilling list ({InterceptList.MaxIntercepts } vs {mergedList.Count}");
    }
  }
}

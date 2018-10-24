using VSS.TRex.Caching;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheStorageTests
  {
    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_AddOneElement()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50);

        storage.Add(new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = 1000,
          OriginX = (uint)(2000),
          OriginY = (uint)(3000)
        });

      Assert.True(storage.HasFreeSpace(), "Storage has no free space when filled with only one element");

      Assert.True(storage.TokenCount == 1, $"Element count incorrect (= {storage.TokenCount})");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_FillWithElements()
    {
      const int numElements = 100;

      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(numElements, numElements / 2);

      // Fill all available slots
      for (int i = 0; i < numElements; i++)
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = 1000,
          OriginX = (uint)(2000 + i * SubGridTreeConsts.SubGridTreeDimension),
          OriginY = (uint)(3000 + i * SubGridTreeConsts.SubGridTreeDimension)
        });

      Assert.False(storage.HasFreeSpace(), "Storage has free space when filled");
      Assert.True(storage.TokenCount == numElements, $"Element count incorrect (= {storage.TokenCount})");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 1)]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    [InlineData(100, 1000)]
    [InlineData(999, 1000)]
    public void Test_TRexSpatialMemoryCacheStorageTests_FillWithElementsThenOverflowBy(int numElements, int overflowBy)
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(numElements, numElements / 2);

      // Fill all available slots
      for (int i = 0; i < numElements; i++)
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = 1000,
          OriginX = (uint)(2000 + i * SubGridTreeConsts.SubGridTreeDimension),
          OriginY = (uint)(3000 + i * SubGridTreeConsts.SubGridTreeDimension)
        });

      Assert.False(storage.HasFreeSpace(), "Storage has free space when filled");

      for (int i = 0; i < overflowBy; i++)
      storage.Add(new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000,
        OriginX = (uint)(2000 + (numElements + i) * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint)(3000 + (numElements + i) * SubGridTreeConsts.SubGridTreeDimension)
      });

      Assert.True(storage.TokenCount == numElements, $"Element count incorrect (= {storage.TokenCount})");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_GetElement()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50);
      var item = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000,
        OriginX = (uint) (2000),
        OriginY = (uint) (3000)
      };

      var index = storage.Add(item);

      Assert.True(storage.TokenCount == 1, $"Element count incorrect (= {storage.TokenCount})");

      var getItem = storage.Get(index);

      Assert.True(ReferenceEquals(item, getItem), $"Item retrieved from storage not same as item placed in storage");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_RemoveOneElement()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50);

      var index = storage.Add(new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000,
        OriginX = (uint)(2000),
        OriginY = (uint)(3000)
      });

      Assert.True(storage.TokenCount == 1, $"Element count incorrect (= {storage.TokenCount})");

      storage.Remove(index);

      Assert.True(storage.TokenCount == 0, $"Element count incorrect (= {storage.TokenCount})");
    }
  }
}

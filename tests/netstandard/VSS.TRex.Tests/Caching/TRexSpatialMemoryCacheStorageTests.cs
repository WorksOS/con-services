﻿using System;
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
          CacheOriginX = (uint)(2000),
          CacheOriginY = (uint)(3000)
        }, null);

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
          CacheOriginX = (uint)(2000 + i * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (uint)(3000 + i * SubGridTreeConsts.SubGridTreeDimension)
        }, null);

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
          CacheOriginX = (uint)(2000 + i * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (uint)(3000 + i * SubGridTreeConsts.SubGridTreeDimension)
        }, null);

      Assert.False(storage.HasFreeSpace(), "Storage has free space when filled");

      for (int i = 0; i < overflowBy; i++)
      storage.Add(new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000,
        CacheOriginX = (uint)(2000 + (numElements + i) * SubGridTreeConsts.SubGridTreeDimension),
        CacheOriginY = (uint)(3000 + (numElements + i) * SubGridTreeConsts.SubGridTreeDimension)
      }, null);

      Assert.True(storage.TokenCount == numElements, $"Element count incorrect (= {storage.TokenCount})");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_GetElement()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50);
      var item = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000,
        CacheOriginX = (uint) (2000),
        CacheOriginY = (uint) (3000)
      };

      var index = storage.Add(item, null);

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
        CacheOriginX = (uint)(2000),
        CacheOriginY = (uint)(3000)
      }, null);

      Assert.True(storage.TokenCount == 1, $"Element count incorrect (= {storage.TokenCount})");

      storage.Remove(index);

      Assert.True(storage.TokenCount == 0, $"Element count incorrect (= {storage.TokenCount})");
    }

    [Theory(Skip="Performance test")]
    [InlineData(100, 1)]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    [InlineData(100, 1000)]
    [InlineData(10000, 10000)]
    [InlineData(10000, 100000)]
    [InlineData(100000, 100000)]
    [InlineData(100000, 1000000)]
    [InlineData(1000000, 10000000)]
    [InlineData(1000000, 100000000)]
    public void Test_TRexSpatialMemoryCacheStorageTests_TimeCheckCreatingElements(int numElements, int overflowBy)
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(numElements, numElements / 2);

      var startTime = DateTime.Now;

      // Fill all available slots
      for (int i = 0; i < numElements; i++)
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = 1000,
          CacheOriginX = (uint)(2000 + i * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (uint)(3000 + i * SubGridTreeConsts.SubGridTreeDimension)
        }, null);

      var midTime = DateTime.Now;

      for (int i = 0; i < overflowBy; i++)
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = 1000,
          CacheOriginX = (uint)(1000 + (numElements + i) * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (uint)(1000 + (numElements + i) * SubGridTreeConsts.SubGridTreeDimension)
        }, null);

      Assert.False(true, $"Time for adding {numElements} elements is {midTime - startTime} and adding {overflowBy} overflows is {DateTime.Now - midTime}");
    }

    [Theory(Skip = "Performance test")]
    [InlineData(100, 1)]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    [InlineData(100, 1000)]
    [InlineData(10000, 10000)]
    [InlineData(10000, 100000)]
    [InlineData(100000, 100000)]
    [InlineData(100000, 1000000)]
    [InlineData(1000000, 10000000)]
    [InlineData(1000000, 100000000)]
    public void Test_TRexSpatialMemoryCacheStorageTests_TimeCheckReusingElements(int numElements, int overflowBy)
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(numElements, numElements / 2);

      var startTime = DateTime.Now;

      var item = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000
      };

      // Fill all available slots
      for (int i = 0; i < numElements; i++)
        storage.Add(item, null);

      var midTime = DateTime.Now;

      for (int i = 0; i < overflowBy; i++)
        storage.Add(item, null);

      Assert.False(true, $"Time for adding {numElements} elements is {midTime - startTime} and adding {overflowBy} overflows is {DateTime.Now - midTime}");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_MaxEpochTokenAgeOnGet()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(1000, 500);

      // Fill half slots 
      for (int i = 0; i < 500; i++)
      { 
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element()
        {
          CacheOriginX = (uint)(1000 + i * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = 1000,
          SizeInBytes = 1000
        }, null);
      }

      var currentMRUHead = storage.MRUHead;
      // Get each item in the same order and verify they are not touched and moved to the MRUHead
      for (int i = 0; i < 500; i++)
      {
        var item = storage.Get(i);

        Assert.True(storage.MRUHead == currentMRUHead, $"MRUHead changed unexpectedly: storage.MRUHead = {storage.MRUHead}, currentMRUHead = {currentMRUHead}");
      }

      // Fill remaining slots 
      for (int i = 500; i < 1000; i++)
      {
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element()
        {
          CacheOriginX = (uint)(1000 + i * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = 1000,
          SizeInBytes = 1000
        }, null);
      }

      // Get each item in the same order and verify they are touched and moved to the MRUHead
      for (int i = 0; i < 1000; i++)
      {
        var item = storage.Get(i);

        Assert.True(storage.MRUHead == i, $"Referenced item not promoted to MRU head at index {i}, MRUHead = {storage.MRUHead}");
      }
    }
  }
}

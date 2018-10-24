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

      Assert.True(storage.TokenCount == numElements, $"Element count incorrect (= {storage.TokenCount})");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_FillWithElementsThenOverflowBy1()
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

      storage.Add(new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = 1000,
        OriginX = (uint)(2000 + numElements * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint)(3000 + numElements * SubGridTreeConsts.SubGridTreeDimension)
      });

      Assert.True(storage.TokenCount == numElements, $"Element count incorrect (= {storage.TokenCount})");
    }
  }
}

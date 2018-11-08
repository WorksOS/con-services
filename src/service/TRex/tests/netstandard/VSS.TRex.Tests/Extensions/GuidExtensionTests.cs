using System;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using Xunit;

namespace VSS.TRex.Tests.Extensions
{
  public class GuidExtensionTests
  {
    [Fact]
    public void Test_SimpleEquality_BothNull()
    {
      Guid[] thisArray = null; 
      Guid[] otherArray = null;

      Assert.True(GuidExtensions.GuidsEqual(thisArray, otherArray));
    }

    [Fact]
    public void Test_SimpleEquality_OneNull()
    {
      Guid[] thisArray = new Guid[0];
      Guid[] otherArray = null;

      Assert.False(thisArray.GuidsEqual(otherArray));
    }

    [Fact]
    public void Test_SimpleEquality_NotNullEmpty()
    {
      Guid[] thisArray = new Guid[0];
      Guid[] otherArray = new Guid[0];

      Assert.True(thisArray.GuidsEqual(otherArray));
    }

    [Fact]
    public void Test_UnevenListSize()
    {
      Guid[] thisArray = new Guid[1];
      Guid[] otherArray = new Guid[2];

      Assert.False(thisArray.GuidsEqual(otherArray));
    }

    [Fact]
    public void Test_UnevenListSize_WithNullContents()
    {
      Guid[] thisArray = new Guid[2];
      Guid[] otherArray = new Guid[2];

      Assert.True(thisArray.GuidsEqual(otherArray));
    }

    [Fact]
    public void Test_UnevenListSize_WitNonNullDifferentContents()
    {
      Guid[] thisArray = new [] {Guid.NewGuid(), Guid.NewGuid()};
      Guid[] otherArray = new [] {Guid.NewGuid(), Guid.NewGuid()};

      Assert.False(thisArray.GuidsEqual(otherArray));
    }

    [Fact]
    public void Test_UnevenListSize_WitNonNullSameContents()
    {
      Guid guid1 = Guid.NewGuid();
      Guid guid2 = Guid.NewGuid();

      Guid[] thisArray = new[] { guid1, guid2 };
      Guid[] otherArray = new[] { guid1, guid2 };

      Assert.True(thisArray.GuidsEqual(otherArray));
    }
  }
}

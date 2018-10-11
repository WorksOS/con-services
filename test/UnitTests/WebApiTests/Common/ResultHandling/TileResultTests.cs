using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Common.ResultHandling
{
  public class TileResultTests
  {
    [TestClass]
    public class EmptyTileTests : TileResultTests
    {
      [TestMethod]
      [DataRow(0, 0)]
      [DataRow(-1, -1)]
      public void Should_throw_When_width_height_are_invalid(int width, int height)
      {
        Assert.ThrowsException<ArgumentException>(() => TileResult.EmptyTile(width, height));
      }

      [TestMethod]
      public void Should_return_empty_object_When_width_height_are_valid()
      {
        var result = TileResult.EmptyTile(45, 67);

        Assert.IsInstanceOfType(result, typeof(TileResult));
        Assert.AreEqual(ContractExecutionResult.DefaultMessage, result.Message);
        Assert.IsNotNull(result.TileData);
      }
    }
  }
}

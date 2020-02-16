using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class EnumTests
  {
    [TestMethod]
    public void BssFailureCode_NotToExceedThirtyChars_DoNotChange()
    {
      foreach (var name in Enum.GetNames(typeof(BssFailureCode)))
      {
        Assert.IsTrue(name.Length <= 30, "BssFailureCode.{0} exceeds 30 chars.", name);
      }
    }
  }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class RequireTests : BssUnitTestBase
  {
    [TestMethod]
    public void IsNotNull_ArgIsNotNull()
    {
      Require.IsNotNull(string.Empty, "");
    }

    [TestMethod]
    public void IsNotNull_NullArgNoMessage_ThrowsWithDefaultMessage()
    {
      bool thrown = false;

      try
      {
        string arg = null;
        Require.IsNotNull(arg, "");
      }
      catch (InvalidOperationException ex)
      {
        Assert.AreEqual("Argument of type String cannot be null.", ex.Message);
        thrown = true;
      }

      Assert.IsTrue(thrown);
    }

    [TestMethod]
    public void IsNotNull_NullArgWithMessage_ThrowsWithMessage()
    {
      bool thrown = false;

      try
      {
        string arg = null;
        Require.IsNotNull(arg, "arg");
      }
      catch (InvalidOperationException ex)
      {
        Assert.AreEqual("arg cannot be null.", ex.Message);
        thrown = true;
      }

      Assert.IsTrue(thrown);
    }
  }
}

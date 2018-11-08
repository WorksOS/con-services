using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.MasterData.Proxies.UnitTests
{
  [TestClass]
  public class StringExtensionsTests
  {
    [TestMethod]
    public void CanCallTruncateWithNullString()
    {
      string str = null;
      var truncStr = str.Truncate(2);
      Assert.IsNull(truncStr);
    }

    [TestMethod]
    public void CanCallTruncateWithZeroMaxLength()
    {
      string str = "some string";
      var truncStr = str.Truncate(0);
      Assert.AreEqual(str, truncStr);
    }

    [TestMethod]
    public void CanTruncateStringWithoutEllipsis()
    {
      string str = "some string";
      var truncStr = str.Truncate(8, false);
      Assert.AreEqual("some str", truncStr);
    }

    [TestMethod]
    public void CanTruncateStringWithEllipsis()
    {
      string str = "some string";
      var truncStr = str.Truncate(8);
      Assert.AreEqual("some ...", truncStr);
    }

    [TestMethod]
    public void NoTruncationForShortString()
    {
      string str = "some string";
      var truncStr = str.Truncate(20);
      Assert.AreEqual(str, truncStr);
    }
  }
}

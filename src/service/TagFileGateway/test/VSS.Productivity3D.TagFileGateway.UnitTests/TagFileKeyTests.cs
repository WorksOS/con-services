using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileGateway.Common.Models.Executors;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  [TestClass]
  public class TagFileKeyTests
  {
    [TestMethod]
    public void TestValidKey()
    {
      var tagFileName = "abc123sn--my machine--161230235959.tag";

      // In s3 we store the tag files under the machine, then the machine with yyMMdd, then the tag file
      var expected = "abc123sn--my machine/abc123sn--my machine--161230/abc123sn--my machine--161230235959.tag";
      var s3Key = TagFileProcessExecutor.GetS3Key(tagFileName);

      Assert.AreEqual(expected, s3Key);
    }

    [TestMethod]
    public void TestInvalidKey()
    {
      var tagFileName = "my invalid tag file.tag";

      // In s3 we store the tag files under the machine, then the machine with yyMMdd, then the tag file
      var expected = "invalid/my invalid tag file.tag";
      var s3Key = TagFileProcessExecutor.GetS3Key(tagFileName);

      Assert.AreEqual(expected, s3Key);
    }
  }
}

using VSS.Productivity3D.TagFileGateway.Common.Executors;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  public class TagFileKeyTests
  {
    [Fact]
    public void TestValidKey()
    {
      var tagFileName = "abc123sn--my machine--161230235959.tag";

      // In s3 we store the tag files under the machine, then the machine with yyMMdd, then the tag file
      var expected = "abc123sn--my machine/abc123sn--my machine--161230/abc123sn--my machine--161230235959.tag";
      var s3Key = TagFileProcessExecutor.GetS3Key(tagFileName);

      Assert.AreEqual(expected, s3Key);
    }

    [Fact]
    public void TestInvalidKey()
    {
      var tagFileName = "my invalid tag file.tag";

      // In s3 we store the tag files under the machine, then the machine with yyMMdd, then the tag file
      var expected = $"{TagFileProcessExecutor.INVALID_TAG_FILE_FOLDER}/my invalid tag file.tag";
      var s3Key = TagFileProcessExecutor.GetS3Key(tagFileName);

      Assert.AreEqual(expected, s3Key);
    }
  }
}

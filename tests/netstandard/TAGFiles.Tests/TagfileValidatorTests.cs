using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TagfileValidatorTests : IClassFixture<DILoggingFixture>

  {

    [Fact]
    public void Test_TFAProxy_Creation()
    {
      IConfiguration Configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();
      TFAProxy tfa = new TFAProxy(Configuration);
      Assert.True(null != tfa, "Failed to construct TFAProxy");
    }

    [Fact]
    public void Test_TFAProxy_BadRequest()
    {
      IConfiguration Configuration = new ConfigurationBuilder()
          .AddEnvironmentVariables()
          .Build();
      TFAProxy tfa = new TFAProxy(Configuration);
      Guid? a;
      Guid? p = null;
      string s;
      int i = 0;
      ValidationResult vr = tfa.ValidateTagfile(Guid.Empty, Guid.Empty, "", 0, 0, 0, DateTime.Now, ref p, out a, out s,ref i);
      Assert.True(vr == ValidationResult.BadRequest, "Failed to return a bad request");
    }


    [Fact]
    public void Test_TagfileValidator_TestInvalidTagFileTooSmall()
    {

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.NewGuid(),
        projectId = Guid.NewGuid(),
        tagFileName = "Test.tag",
        tagFileContent = new byte[1],
        tccOrgId = "",
        IsJohnDoe = false
      };

      string s;
      int i = 0;
      // Validate tagfile submission
      var result = TagfileValidator.ValidSubmission(td, out s, out i);

      Assert.True(result == ValidationResult.InvalidTagfile, "Failed to return a Invalid request");
    }

    [Fact]
    public void Test_TagfileValidator_TestInvalidEmptyTagFile()
    {

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.NewGuid(),
        projectId = Guid.NewGuid(),
        tagFileName = "Test.tag",
        tagFileContent = new byte[101],
        tccOrgId = "",
        IsJohnDoe = false
      };
      string s;
      // Validate tagfile submission
      int i = 0;
      var result = TagfileValidator.ValidSubmission(td, out s, out i);

      Assert.True(result == ValidationResult.InvalidTagfile, "Failed to return a Invalid request");
    }


    [Fact]
    public void Test_TagfileValidator_TestValidTagFile()
    {

      byte[] tagContent;
      using (FileStream tagFileStream =
              new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
                      FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int)tagFileStream.Length);
      }

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.NewGuid(),
        projectId = Guid.NewGuid(),
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };
      string s;
      int i = 0;
      // Validate tagfile submission
      var result = TagfileValidator.ValidSubmission(td, out s, out i);

      Assert.True(result == ValidationResult.Valid, "Failed to return a Valid request");
    }


    [Fact]
    public void Test_TagfileValidator_TestTagFileArchive()
    {

      byte[] tagContent;
      using (FileStream tagFileStream =
              new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"),
                      FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int)tagFileStream.Length);
      }

      TagFileDetail td = new TagFileDetail()
      {
        assetId = Guid.Parse("{00000000-0000-0000-0000-000000000001}"),
        projectId = Guid.Parse("{00000000-0000-0000-0000-000000000001}"),
        tagFileName = "Test.tag",
        tagFileContent = tagContent,
        tccOrgId = "",
        IsJohnDoe = false
      };

      Assert.True(TagFileRepository.ArchiveTagfile(td), "Failed to archive tagfile");
    }

    // Possibly have a future test with a mocked TFAProxy
  }
}

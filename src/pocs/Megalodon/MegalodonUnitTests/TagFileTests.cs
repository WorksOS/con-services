using Microsoft.Extensions.Logging;
using TagFiles;
using VSS.Common.Abstractions.Configuration;
using FluentAssertions;
using Xunit;
using TagFiles.Common;

namespace MegalodonUnitTests
{
  public class TagFileTests
  {
    TagFile tagFile;

    public void SetupParser()
    {
      var logF = new LoggerFactory();
      IConfigurationStore config = new TestingConfig(logF);
      var log = logF.CreateLogger<TestingConfig>();
      tagFile = new TagFile();
      tagFile.SetupLog(log);
    }

    [Fact]
    public void CreateTagFileClass()
    {
      var tagfile = new TagFile();
      tagfile.Should().NotBeNull("tagfile is Null");
      tagfile.Parser.Should().NotBeNull("parser is Null");
    }

    [Fact]
    public void DictionaryCreated()
    {
      var tagfile = new TagFile();
      tagfile.TagFileDictionary.Should().NotBeNull("TagFileDictionary is Null");
      tagfile.TagFileDictionary.Entries.Count.Should().BeGreaterThan(25);
    }

    [Fact]
    public void ParseInvalidEmptyText()
    {
      SetupParser();
      var res = tagFile.ParseText("");
      res.Should().BeFalse("Result should be false. Empty string");
      res = tagFile.ParseText(TagConstants.CHAR_STX + TagConstants.BLADE_ON_GROUND + "1" + TagConstants.CHAR_ETX);
      res.Should().BeFalse("Result should be false. Missing record seperator ");
    }

    [Fact]
    public void ParseValidBOG()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.BLADE_ON_GROUND +"1");
      res.Should().BeTrue("Valid BOG. Result should be true");
    }

    [Fact]
    public void ParseInValidBOG()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.BLADE_ON_GROUND + "A");
      res.Should().BeFalse("Invalid BOG. Result should be false");
    }


  }
}

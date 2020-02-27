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

    [Fact]
    public void ParseValidTime()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.TIME + "1574216737695");
      res.Should().BeTrue("Valid Time. Result should be true");
    }

    [Fact]
    public void ParseInValidTime()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.TIME + "A");
      res.Should().BeFalse("Invalid Time. Result should be false");
    }

    [Fact]
    public void ParseValidLEB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LEFT_EASTING_BLADE + "1576594.3");
      res.Should().BeTrue("Valid LEB. Result should be true");
    }

    [Fact]
    public void ParseInValidLEB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LEFT_EASTING_BLADE + "A");
      res.Should().BeFalse("Invalid LEB. Result should be false");
    }

    [Fact]
    public void ParseValidLNB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LEFT_NORTHING_BLADE + "5177503");
      res.Should().BeTrue("Valid LNB. Result should be true");
    }

    [Fact]
    public void ParseInValidLNB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LEFT_NORTHING_BLADE + "A");
      res.Should().BeFalse("Invalid LNB. Result should be false");
    }

    [Fact]
    public void ParseValidLHB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LEFT_HEIGHT_BLADE + "-3.0");
      res.Should().BeTrue("Valid LHB. Result should be true");
    }

    [Fact]
    public void ParseInValidLHB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LEFT_HEIGHT_BLADE + "A");
      res.Should().BeFalse("Invalid LHB. Result should be false");
    }

    [Fact]
    public void ParseValidREB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.RIGHT_EASTING_BLADE + "1576597.3");
      res.Should().BeTrue("Valid REB. Result should be true");
    }

    [Fact]
    public void ParseInValidREB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.RIGHT_EASTING_BLADE + "A");
      res.Should().BeFalse("Invalid REB. Result should be false");
    }

    [Fact]
    public void ParseValidRNB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.RIGHT_NORTHING_BLADE + "1");
      res.Should().BeTrue("Valid RNB. Result should be true");
    }

    [Fact]
    public void ParseInValidRNB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.RIGHT_NORTHING_BLADE + "A");
      res.Should().BeFalse("Invalid RNB. Result should be false");
    }

    [Fact]
    public void ParseValidRHB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.RIGHT_HEIGHT_BLADE + "-3.0");
      res.Should().BeTrue("Valid RHB. Result should be true");
    }

    [Fact]
    public void ParseInValidRHB()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.RIGHT_HEIGHT_BLADE + "A");
      res.Should().BeFalse("Invalid RHB. Result should be false");
    }

    [Fact]
    public void ParseValidGPM()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.GPS_MODE + "3");
      res.Should().BeTrue("Valid GPS. Result should be true");
    }

    [Fact]
    public void ParseInValidGPM()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.GPS_MODE + "A");
      res.Should().BeFalse("Invalid GPS. Result should be false");
    }

    [Fact]
    public void ParseValidDES()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.DESIGN + "Design A");
      res.Should().BeTrue("Valid Design. Result should be true");
    }


    [Fact]
    public void ParseValidLAT()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LATITUDE + "-0.760183007721451");
      res.Should().BeTrue("Valid LAT. Result should be true");
    }

    [Fact]
    public void ParseInValidLAT()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LATITUDE + "A");
      res.Should().BeFalse("Invalid LAT. Result should be false");
    }

    [Fact]
    public void ParseValidLON()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LONTITUDE + "3.01435300239811");
      res.Should().BeTrue("Valid LON. Result should be true");
    }

    [Fact]
    public void ParseInValidLON()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.LONTITUDE + "A");
      res.Should().BeFalse("Invalid LON. Result should be false");
    }

    [Fact]
    public void ParseValidHGT()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.HEIGHT + "-3.0");
      res.Should().BeTrue("Valid HGT. Result should be true");
    }

    [Fact]
    public void ParseInValidHGT()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.HEIGHT + "A");
      res.Should().BeFalse("Invalid HGT. Result should be false");
    }

    [Fact]
    public void ParseValidMID()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MACHINE_ID + "Sandpiper");
      res.Should().BeTrue("Valid MID. Result should be true");
    }

    [Fact]
    public void ParseValidMSD()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MACHINE_SPEED + "1.0");
      res.Should().BeTrue("Valid MSD. Result should be true");
    }

    [Fact]
    public void ParseInValidMSD()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MACHINE_SPEED + "A");
      res.Should().BeFalse("Invalid MSD. Result should be false");
    }

    [Fact]
    public void ParseValidMTP()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MACHINE_TYPE + "CSD");
      res.Should().BeTrue("Valid MTP. Result should be true");
    }

    [Fact]
    public void ParseInValidMTP()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MACHINE_TYPE + "A");
      res.Should().BeFalse("Invalid MTP. Result should be false");
    }

    [Fact]
    public void ParseValidHDG()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.HEADING + "118.0");
      res.Should().BeTrue("Valid HDG. Result should be true");
    }

    [Fact]
    public void ParseInValidHDG()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.HEADING + "A");
      res.Should().BeFalse("Invalid HDG. Result should be false");
    }

    [Fact]
    public void ParseValidSER()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.SERIAL + "7g2b0b13-5b31-4ac7-b67a-4f16d76c9b30<RS>MTPCSD");
      res.Should().BeTrue("Valid SER. Result should be true");
    }

    [Fact]
    public void ParseValidUTM()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.UTM + "0");
      res.Should().BeTrue("Valid UTM. Result should be true");
    }

    [Fact]
    public void ParseInValidUTM()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.UTM + "A");
      res.Should().BeFalse("Invalid UTM. Result should be false");
    }


  }
}

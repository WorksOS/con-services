using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TagFiles;
using TagFiles.Common;
using TagFiles.Utils;
using Xunit;

namespace UnitTests
{
  public class TagFileTests
  {
    TagFile tagFile;

    private void SetupParser()
    {
      var logF = new LoggerFactory();
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
      tagfile.TagFileDictionary.Entries.Count.Should().Be(38);
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
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.BLADE_ON_GROUND + "1");
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

    [Fact]
    public void ParseValidCCV()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.CCV + "100");
      res.Should().BeTrue("Valid CCV. Result should be true");
    }

    [Fact]
    public void ParseValidMDP()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MDP + "100");
      res.Should().BeTrue("Valid MDP. Result should be true");
    }

    [Fact]
    public void ParseValidTargetMDP()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.MDP + "100");
      res.Should().BeTrue("Valid TargetMDP. Result should be true");
    }

    [Fact]
    public void ParseValidTargetThickness()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.TARGET_THICKNESS + "100");
      res.Should().BeTrue("Valid Targetthickness. Result should be true");
    }

    [Fact]
    public void ParseValidTargetPasses()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.TARGET_PASSCOUNT + "10");
      res.Should().BeTrue("Valid TargetPassses. Result should be true");
    }

    [Fact]
    public void ParseValidTempMin()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.TEMP_MIN + "100");
      res.Should().BeTrue("Valid Temp Min.  Result should be true");
    }

    [Fact]
    public void ParseValidTempMax()
    {
      SetupParser();
      var res = tagFile.ParseText(TagConstants.CHAR_RS + TagConstants.TEMP_MAX + "100");
      res.Should().BeTrue("Valid Temp Max.  Result should be true");
    }

    /// <summary>
    /// Example of custom tagfile creation
    /// </summary>
    [Fact(Skip = "Developer Only")]
    public void TestTagFileCreation()
    {
      SetupParser();
      tagFile.SetupDefaultConfiguration(0);

      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
      var timeStamp = "TME" + unixTimestamp.ToString();
      
      var rs = Convert.ToChar(TagConstants.RS).ToString();

      var hdr = rs + timeStamp + rs + "HDR1" + rs + "GPM3" + rs + "DESDesign A" + rs + "LAT0.631930995750444" + rs + "LON-2.007479992025198" + rs + "HGT542" + rs + "MIDTestTagFile" + rs + "UTM0" + rs + "HDG90" + rs +
                   "SER1551J025SW" + rs + "MTPCOM" + rs + "BOG0" + rs + "MPM0" + rs + "CST1" + rs + "FLG4" + rs + "TMP900" + rs  + "TCC600" + rs + "DIR1" + rs + "TTS200" + rs + "TPC5" + rs + "TMD200" + rs + "TMN90" + rs + "TMX143";
      tagFile.ParseText(hdr);

      unixTimestamp += 100; timeStamp = "TME" + unixTimestamp.ToString();
      var rec = rs + timeStamp+ rs + "HDR0" + rs + "LEB2745" + rs + "LNB1163.5" + rs + "LHB542" + rs + "REB2745" + rs + "RNB1163" + rs + "RHB542" + rs + "BOG1" + rs + "HDG90" + rs + "CCV600" + rs + "MDP210";
      tagFile.ParseText(rec);

      unixTimestamp += 100; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2746" + rs + "LNB1163.5" + rs + "LHB543.5" + rs + "REB2746" + rs + "RNB1163" + rs + "RHB543.5" + rs + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      unixTimestamp += 100; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2747" + rs + "LNB1163.5" + rs + "LHB544" + rs + "REB2747" + rs + "RNB1163" + rs + "RHB544" + rs + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      unixTimestamp += 100; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2748" + rs + "LNB1163.5" + rs + "LHB545" + rs + "REB2748" + rs + "RNB1163" + rs + "RHB545" + rs + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      tagFile.WriteTagFileToDisk(); // output to standard trimble install location

    }

    /// <summary>
    /// Example of custom tagfile creation
    /// </summary>
    [Fact(Skip = "Developer Only")]
    public void CreateStandardTagFile()
    {
      SetupParser();
      tagFile.SetupDefaultConfiguration(0);
      var hgt = "HGT545.0";
      var lhb = "LHB545.0";
      var rhb = "RHB545.0";
      long timeGap = 1500; // good gap to see in DDV

      var unixTimestamp = TagUtils.GetCurrentUnixTimestampMillis();
      var timeStamp = "TME" + unixTimestamp.ToString();

      var rs = Convert.ToChar(TagConstants.RS).ToString();

      // Setup a compactor header but lets add as much info as possible even if not realistic
      var hdr = rs + timeStamp + rs + "HDR1" + rs + "GPM3" + rs + "DESDesign A" + rs + "LAT0.631930995750444" + rs + "LON-2.007479992025198" + rs + hgt + rs + "MIDStandardTestTagFile" + rs + "UTM0" + rs + "HDG90" + rs +
        "SER00000000-0000-0000-0000-000000000001" + rs + "MTPCOM" + rs + "BOG0" + rs+ "MPM0" + rs + "CST1" + rs + "FLG4" + rs + "TMP920" + rs + "TCC600" + rs + "DIR1" +
         rs + "TTS200" + rs + "TPC5" + rs + "TMD200" + rs + "TMN90" + rs + "TMX143";

      tagFile.ParseText(hdr);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      var rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2745" + rs + "LNB1163.5" + rs + lhb + rs + "REB2745" + rs + "RNB1163" + rs + rhb + rs + "BOG1" + rs + "HDG90" + rs + "CCV600" + rs + "MDP190" + rs + "TMP900";
      tagFile.ParseText(rec);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2746" + rs + "LNB1163.5" + rs + lhb + rs + "REB2746" + rs + "RNB1163" + rs + rhb + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2747" + rs + "LNB1163.5" + rs + lhb + rs + "REB2747" + rs + "RNB1163" + rs + rhb + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2748" + rs + "LNB1163.5" + rs + lhb + rs + "REB2748" + rs + "RNB1163" + rs + rhb + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      // Lift the blade off ground so to speak
      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "BOG0";
      tagFile.ParseText(rec);

      // now repeat sames passes with new values

      lhb = "LHB546";
      rhb = "RHB546";

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2745" + rs + "LNB1163.5" + rs + lhb + rs + "REB2745" + rs + "RNB1163" + rs + rhb + rs + "BOG1" + rs + "HDG90" + rs + "CCV610" + rs + "MDP210" + rs + "TMP920";
      tagFile.ParseText(rec);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2746" + rs + "LNB1163.5" + rs + lhb + rs + "REB2746" + rs + "RNB1163" + rs + rhb + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2747" + rs + "LNB1163.5" + rs + lhb + rs + "REB2747" + rs + "RNB1163" + rs + rhb + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      unixTimestamp += timeGap; timeStamp = "TME" + unixTimestamp.ToString();
      rec = rs + timeStamp + rs + "HDR0" + rs + "LEB2748" + rs + "LNB1163.5" + rs + lhb + rs + "REB2748" + rs + "RNB1163" + rs + rhb + "BOG1" + rs + "HDG90";
      tagFile.ParseText(rec);

      tagFile.WriteTagFileToDisk();

    }

  }
}

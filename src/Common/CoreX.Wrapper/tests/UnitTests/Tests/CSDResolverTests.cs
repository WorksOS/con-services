using System.Collections.Generic;
using CoreX.Interfaces;
using CoreX.Wrapper.UnitTests.ExpectedTestResultObjects;
using CoreX.Wrapper.UnitTests.Types;
using CoreXModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Org.BouncyCastle.Asn1.Misc;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CSDResolverTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly UnitTestBaseFixture _fixture;
    private readonly ICoreXWrapper _coreX;

    public CSDResolverTests(UnitTestBaseFixture testFixture)
    {
      _fixture = testFixture;
      _coreX = testFixture.CoreXWrapper;
    }

    [Theory]
    [InlineData(null, "DC file string cannot be null (Parameter 'dcFileStr')")]
    [InlineData(DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST, "Error 'cecDCParseLineNotEndingWithLF' attempting to retrieve the DC file's CSD")]
    public void Should_throw_for_badly_formatted_file_string(string dcFileString, string expectedErrorMessage)
    {
      var ex = Record.Exception(() => _coreX.GetCSDFromDCFileContent(dcFileString));

      ex.Message.Should().Be(expectedErrorMessage);
    }

    [Theory]
    [MemberData(nameof(GetCSDData))]
    public void Should_return_CoordinateSystem_for_valid_DC_file_string(string dcFileString, CoordinateSystem expectedCS)
    {
      var result = _coreX.GetCSDFromDCFileContent(_fixture.GetDCFileContent(dcFileString));

      result.Should().NotBeNull();
      result.GeoidInfo.Should().NotBeNull();
      result.DatumInfo.Should().NotBeNull();
      result.ZoneInfo.Should().NotBeNull();

      result.SystemName.Should().Be(expectedCS.SystemName);

      result.GeoidInfo.GeoidFileName.Should().Be(expectedCS.GeoidInfo.GeoidFileName);
      result.GeoidInfo.GeoidName.Should().Be(expectedCS.GeoidInfo.GeoidName);
      result.GeoidInfo.GeoidSystemId.Should().Be(expectedCS.GeoidInfo.GeoidSystemId);

      result.DatumInfo.DatumName.Should().Be(expectedCS.DatumInfo.DatumName);
      result.DatumInfo.DatumType.Should().Be(expectedCS.DatumInfo.DatumType);
      result.DatumInfo.DatumSystemId.Should().Be(expectedCS.DatumInfo.DatumSystemId);

      result.ZoneInfo.ZoneType.Should().Be(expectedCS.ZoneInfo.ZoneType);
      result.ZoneInfo.ShiftGridFileName.Should().Be(expectedCS.ZoneInfo.ShiftGridFileName);
      result.ZoneInfo.SnakeGridFileName.Should().Be(expectedCS.ZoneInfo.SnakeGridFileName);
      result.ZoneInfo.IsSouthGrid.Should().Be(expectedCS.ZoneInfo.IsSouthGrid);
      result.ZoneInfo.IsWestGrid.Should().Be(expectedCS.ZoneInfo.IsWestGrid);
      result.ZoneInfo.OriginLatitude.Should().Be(expectedCS.ZoneInfo.OriginLatitude);
      result.ZoneInfo.OriginLongitude.Should().Be(expectedCS.ZoneInfo.OriginLongitude);
      result.ZoneInfo.OriginNorth.Should().Be(expectedCS.ZoneInfo.OriginNorth);
      result.ZoneInfo.OriginEast.Should().Be(expectedCS.ZoneInfo.OriginEast);
      result.ZoneInfo.OriginScale.Should().Be(expectedCS.ZoneInfo.OriginScale);

      if (result.ZoneInfo.HorizontalAdjustment != null)
      {
        result.ZoneInfo.HorizontalAdjustment.OriginEast.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.OriginEast);
        result.ZoneInfo.HorizontalAdjustment.OriginNorth.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.OriginNorth);
        result.ZoneInfo.HorizontalAdjustment.Rotation.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.Rotation);
        result.ZoneInfo.HorizontalAdjustment.Scale.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.Scale);
        result.ZoneInfo.HorizontalAdjustment.TranslationEast.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.TranslationEast);
        result.ZoneInfo.HorizontalAdjustment.TranslationNorth.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.TranslationNorth);
        result.ZoneInfo.HorizontalAdjustment.IsNullAdjustment.Should().Be(expectedCS.ZoneInfo.HorizontalAdjustment.IsNullAdjustment);
      }

      if (result.ZoneInfo.VerticalAdjustment != null)
      {
        result.ZoneInfo.VerticalAdjustment.Should().NotBeNull();
        result.ZoneInfo.VerticalAdjustment.ConstantAdjustment.Should().Be(expectedCS.ZoneInfo.VerticalAdjustment.ConstantAdjustment);
        result.ZoneInfo.VerticalAdjustment.OriginEast.Should().Be(expectedCS.ZoneInfo.VerticalAdjustment.OriginEast);
        result.ZoneInfo.VerticalAdjustment.OriginNorth.Should().Be(expectedCS.ZoneInfo.VerticalAdjustment.OriginNorth);
        result.ZoneInfo.VerticalAdjustment.SlopeEast.Should().Be(expectedCS.ZoneInfo.VerticalAdjustment.SlopeEast);
        result.ZoneInfo.VerticalAdjustment.SlopeNorth.Should().Be(expectedCS.ZoneInfo.VerticalAdjustment.SlopeNorth);
        result.ZoneInfo.VerticalAdjustment.IsNullAdjustment.Should().Be(expectedCS.ZoneInfo.VerticalAdjustment.IsNullAdjustment);
      }
    }

    public static IEnumerable<object[]> GetCSDData() =>
      new List<object[]>
      {
        new object[] { DCFile.NETHERLANDS_DE_MIN, ExpectedCSDResults.Netherlaneds_With_Geoid },
        new object[] { DCFile.FLORIDA_EAST_0901_NAD_1983, ExpectedCSDResults.Florida_East_0901_NAD_1983_No_Geoid }
      };
  }
}

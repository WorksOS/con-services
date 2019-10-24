using System;
using System.IO;
using Xunit;

namespace VSS.DataOcean.Client.UnitTests
{
  public class DataOceanFileUtilTests
  {
    [Theory]
    [InlineData("dxf")]
    [InlineData("DXF")]
    [InlineData("tiff")]
    [InlineData("TIFF")]
    public void CanCreateValidDataOceanFileUtil(string extension)
    {
      var ex = Record.Exception(() => new DataOceanFileUtil($"/dev/folder-one/folder-two/dummy.{extension}"));
      Assert.Null(ex);
    }

    [Fact]
    public void CannotCreateInvalidDataOceanFileUtil()
    {
      Assert.Throws<ArgumentException>(() => new DataOceanFileUtil("/dev/folder-one/folder-two/dummy.ttm"));
    }

    [Theory]
    [InlineData("dxf")]
    [InlineData("DXF")]
    [InlineData("tiff")]
    [InlineData("TIFF")]
    public void CanGetTileMetadataFileName(string extension)
    {
      var pathAndName = $"{DataOceanUtil.PathSeparator}dev{DataOceanUtil.PathSeparator}folder-one{DataOceanUtil.PathSeparator}folder-two{DataOceanUtil.PathSeparator}dummy";
      var fullFileName = $"{pathAndName}.{extension}";
      var file = new DataOceanFileUtil(fullFileName);
      var metadataName = file.TilesMetadataFileName;

      var expectedName = "dxf".Equals(extension, StringComparison.OrdinalIgnoreCase) ? "tiles" : "xyz";
      var expectedMetadata = $"{pathAndName}{DataOceanFileUtil.GENERATED_TILE_FOLDER_SUFFIX}/tiles/{expectedName}.json";

      Assert.Equal(expectedMetadata, metadataName);
    }

    [Theory]
    [InlineData("some name.dxf")]
    [InlineData("下絵.dxf")]
    public void DataOceanFileNameShouldContainAGuid(string fileName)
    {
      var fileUid = Guid.NewGuid();
      var dataOceanFileName = DataOceanFileUtil.DataOceanFileName(fileName, false, fileUid, null);
      Assert.StartsWith(fileUid.ToString(), dataOceanFileName);
    }

    [Fact]
    public void DataOceanFileNameForGeotiffShouldContainSurveyedUtc()
    {
      var fileUid = Guid.NewGuid();
      var surveyedUtc = DateTime.UtcNow;
      var dataOceanFileName = DataOceanFileUtil.DataOceanFileName("some name.tif", true, fileUid, surveyedUtc);
      Assert.StartsWith(fileUid.ToString(), dataOceanFileName);
      var datePart = Path.GetFileNameWithoutExtension(dataOceanFileName).Substring(fileUid.ToString().Length);
      Assert.False(string.IsNullOrEmpty(datePart));
    }
  }
}

using System;
using System.Text;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoreXWrapperTests
  {
    [Fact]
    public void SetCSIBFromDCFile_should_return_invalidLength_for_null_DC_content()
    {
      var exObj = Assert.Throws<ArgumentException>(() => CoreX.GetCSIBFromDCFile(string.Empty));

      exObj.Message.Should().Be("Empty path name is not legal. (Parameter 'path')");
    }

    [Fact]
    public void GetCSIBFromDCFile_should_return_expected_CSIB_string()
    {
      var csibStr = CoreX.GetCSIBFromDCFile(DCFile.GetFilePath(DCFile.UTM_32_NN1954_08));

      csibStr.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeodeticX_geoCreateTransformer_should_fail_When_CSIB_is_invalid()
    {
      var ex = Record.Exception(() => new CoreX().TransformLLHToNEE(
        TestConsts.DIMENSIONS_2012_DC_COORDINATE_SYSTEM_ID, new LLH(), CoordinateTypes.LocalLLE, CoordinateTypes.NormalizedNEE));

      ex.Message.Should().Be("Failed to create GeodeticX transformer, error 'gecCSIB_INVALID_CSIB'");
    }
  }
}

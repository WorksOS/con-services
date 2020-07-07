using System;
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
  }
}

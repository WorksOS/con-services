using System;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoreXWrapperTests
  {
    private readonly CoreX _coreX = new CoreX();

    [Fact]
    public void SetCSIBFromDCFile_should_return_invalidLength_for_null_DC_content()
    {
      var exObj = Assert.Throws<ArgumentException>(() => _coreX.SetCSIBFromDCFile(string.Empty));

      exObj.Message.Should().Be("Empty path name is not legal. (Parameter 'path')");
    }

    [Fact]
    public void SetCSIBFromDCFile_should_return_expected_CSIB_string()
    {
      _coreX.SetCSIBFromDCFile(DCFile.GetFilePath(DCFile.UTM_32_NN1954_08));

      _coreX.CSIB.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetCSIBFromDCFile_should_return_expected_CSIB_string()
    {
      var csibStr = _coreX.GetCSIBFromDCFile(DCFile.GetFilePath(DCFile.UTM_32_NN1954_08));

      csibStr.Should().NotBeNullOrEmpty();
    }
  }
}

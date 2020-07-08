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
    public void GetCSIBFromDCFile_should_return_expected_CSIB_string2()
    {
      var content = "MDBUTVNDIFYxMC03MCAgICAgICAwICAgMTEvMDEvMjAxMiAxMToyNTEzMzExMQ0KMTBUTVVudGl0bGVkIEpvYiAgICAxMjIyMTINCjc4VE0xMQ0KRDVUTSAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA0KRDhUTSAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgDQo2NFRNMzM2LjIwNjU1NTM3MTAwMDAtMTE1LjAyNjI2NzgxODAwMC4wMDAwMDAwMDAwMDAwMDM2NzMuNzA4MDAwMDAwMDA3MTk4LjA4MTAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDEuMDAwMDg2NzIzMDAwMDAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA0KNjVUTTIwOTI1NjA0LjQ3NDE3MDAyOTguMjU3MjIyOTMyODkwDQo0OVRNMzIwOTI1NjA0LjQ3NDE2NzAyOTguMjU3MjIzNTYzMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMA0KNTBUTTM5MzMuMDU0MjcwMDAwMDA4MTc0LjE0MDEyMDAwMDAwMC4wMDg3MzAwMDAwMDAwMDAuMDAwNDUwMDAwMDAwMDAwLjAwMTkwNzk5NzAwMDAwMS4wMDAwMTMwMTMwMDAwMA0KQzhUTTRTQ1M5MDAgTG9jYWxpemF0aW9uICAgICAgICAgICAgIFNDUzkwMCBSZWNvcmQgICAgICAgICAgICAgICAgICAgV0dTODQgRXF1aXZhbGVudCBEYXR1bSAgICAgICAgICANCg==";
      var csibStr = CoreX.GetCSIBFromDCFileContent(content);

      csibStr.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeodeticX_geoCreateTransformer_should_fail_When_CSIB_is_invalid()
    {
      var ex = Record.Exception(() => new CoreX().TransformLLHToNEE(
        TestConsts.DIMENSIONS_2012_DC_COORDINATE_SYSTEM_ID, new LLH(), CoordinateTypes.LocalLLE, CoordinateTypes.NormalizedNEE));

      ex.Message.Should().Be("Failed to create GeodeticX transformer, error 'gecCSIB_INVALID_CSIB'");
    }

    [Theory]
    [InlineData(TestConsts.ddd, true)]
    [InlineData(TestConsts.DIMENSIONS_2012_DC_CSIB, true)]
    [InlineData(TestConsts.DIMENSIONS_2012_DC_COORDINATE_SYSTEM_ID, false)]
    public void Validate_CSIB_strings(string csibCandiate, bool expectedResult)
    {
      CoreX.ValidateCsibString(csibCandiate)
           .Should()
           .Be(expectedResult);
    }
  }
}

using System;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Utilities
{
  public class TRNHelperTests
  {

    [Theory]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void ExtractGuidFromTRNTest(string expectedGuidString, string TRNtype, string userTRN)
    {
      var convertedGuid = TRNHelper.ExtractGuid(userTRN);
      Assert.Equal(expectedGuidString, convertedGuid.ToString());
    }

    [Theory]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void ExtractGuidFromTRNAsStringTest(string expectedGuidString, string TRNtype, string userTRN)
    {
      var convertedGuidString = TRNHelper.ExtractGuidAsString(userTRN);
      Assert.Equal(expectedGuidString, convertedGuidString);
    }

    [Theory]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void MakeTRNFromGuidStringTest(string guidString, string TRNtype, string expectedUserTRN)
    { 
      var convertedUserTRN = TRNHelper.MakeTRN(guidString, TRNtype);
      Assert.Equal(expectedUserTRN, convertedUserTRN);
    }

    [Theory]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [InlineData("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void MakeTRNFromGuidTest(string guidString, string TRNtype, string expectedUserTRN)
    {
      var guid = new Guid(guidString);
      var convertedUserTRN = TRNHelper.MakeTRN(guid, TRNtype);
      Assert.Equal(expectedUserTRN, convertedUserTRN);
    }

  }
}


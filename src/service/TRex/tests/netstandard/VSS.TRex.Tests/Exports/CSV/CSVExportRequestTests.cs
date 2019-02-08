using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.MasterData.Models.FIlters;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportRequestTests : IClassFixture<DITagFileFixture>
  {
    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "gotAFilename.csv", 
      CoordType.LatLon, OutputTypes.VedaAllPasses, null)]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "gotAFilename.csv",
      CoordType.Northeast, OutputTypes.VedaFinalPass, new string[] { "GoodMachineName", "another one" })]
    public void VetaExportRequest_Successful(
      Guid projectUid, FilterResult filter, string fileName,
      CoordType coordType, OutputTypes outputType, string[] machineNames)
    {
      var request = CompactionVetaExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, machineNames);
      request.Validate();
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "",
      CoordType.LatLon, OutputTypes.VedaAllPasses, null)]
    public void VetaExportRequest_FilenameUnSuccessful(
      Guid projectUid, FilterResult filter, string fileName,
      CoordType coordType, OutputTypes outputType, string[] machineNames)
    {
      var request = CompactionVetaExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, machineNames);

      var validate = new ValidFilenameAttribute(256);
      var result = validate.IsValid(request.FileName);
      Assert.False(result);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", null, "somefilename",
      CoordType.LatLon, OutputTypes.VedaAllPasses, null,
      "Invalid project UID.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "somefilename",
      CoordType.LatLon, OutputTypes.PassCountAllPasses, null,
      "Invalid output type for veta export")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "somefilename",
      CoordType.LatLon, OutputTypes.VedaAllPasses, new string[] { "shrt" },
      "Invalid machineNames")]
    public void VetaExportRequest_UnSuccessful(
      Guid projectUid, FilterResult filter, string fileName,
      CoordType coordType, OutputTypes outputType, string[] machineNames,
      string errorMessage)
    {
      var request = CompactionVetaExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, machineNames);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }

    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "gotAFilename",
     CoordType.LatLon, OutputTypes.PassCountAllPasses, true, true)]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "gotAFilename",
     CoordType.Northeast, OutputTypes.PassCountLastPass, false, false)]
    public void PassCountExportRequest_Successful(
     Guid projectUid, FilterResult filter, string fileName,
     CoordType coordType, OutputTypes outputType, bool restrictOutputSize, bool rawDataAsDBase)
    {
      var request = CompactionPassCountExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, restrictOutputSize, rawDataAsDBase);
      request.Validate();
    }
   
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", null, "somefilename",
      CoordType.LatLon, OutputTypes.PassCountAllPasses, false, false,
      "Invalid project UID.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "somefilename",
      CoordType.LatLon, OutputTypes.VedaAllPasses, false, false,
      "Invalid output type for passCount export")]
    public void PassCountExportRequest_UnSuccessful(
      Guid projectUid, FilterResult filter, string fileName,
      CoordType coordType, OutputTypes outputType, bool restrictOutputSize, bool rawDataAsDBase,
      string errorMessage)
    {
      var request = CompactionPassCountExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, restrictOutputSize, rawDataAsDBase);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }
  }
}



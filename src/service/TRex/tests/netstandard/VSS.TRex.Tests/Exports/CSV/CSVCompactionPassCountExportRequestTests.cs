using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVCompactionPassCountExportRequestTests : IClassFixture<DITagFileFixture>
  {
    [Theory]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "gotAFilename",
     CoordType.LatLon, OutputTypes.PassCountAllPasses, true, true)]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "gotAFilename",
     CoordType.Northeast, OutputTypes.PassCountLastPass, false, false)]
    public void PassCountExportRequest_Successful(
     Guid projectUid, FilterResult filter, string fileName,
     CoordType coordType, OutputTypes outputType, bool restrictOutputSize, bool rawDataAsDBase)
    {
      var userPreferences = new UserPreferences();
      var request = new CompactionPassCountExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreferences, restrictOutputSize, rawDataAsDBase);
      request.Validate();
    }
   
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", null, "somefilename",
      CoordType.LatLon, OutputTypes.PassCountAllPasses, false, false,
      "Invalid project UID.")]
    [InlineData("87e6bd66-54d8-4651-8907-88b15d81b2d7", null, "somefilename",
      CoordType.LatLon, OutputTypes.VedaAllPasses, false, false,
      "Invalid output type for pass count export")]
    public void PassCountExportRequest_UnSuccessful(
      Guid projectUid, FilterResult filter, string fileName,
      CoordType coordType, OutputTypes outputType, bool restrictOutputSize, bool rawDataAsDBase,
      string errorMessage)
    {
      var userPreferences = new UserPreferences();
      var request = new CompactionPassCountExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreferences, restrictOutputSize, rawDataAsDBase);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code);
      Assert.Equal(errorMessage, ex.GetResult.Message);
    }
  }
}



using System;
using CCSS.Geometry;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectValidationTBCTests : UnitTestsDIFixture<ProjectValidationTBCTests>
  {
    private static BusinessCenterFile _businessCenterFile;

    public ProjectValidationTBCTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _businessCenterFile = new BusinessCenterFile
      {
        FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01",
        Path = "/BC Data/Sites/Chch Test Site",
        Name = "CTCTSITECAL.dc",
        CreatedUtc = DateTime.UtcNow.AddDays(-0.5)
      };
    }


    [Fact]
    public void ValidateCreateProjectV5Request_CheckBusinessCentreFile()
    {
      var bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "BC Data/Sites/Chch Test Site/";

      var resultantBusinessCenterFile = ProjectDataValidator.ValidateBusinessCentreFile(bcf);
      Assert.Equal("/BC Data/Sites/Chch Test Site", resultantBusinessCenterFile.Path);

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "";
      var ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2083", StringComparison.Ordinal));

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Name = "";
      ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2002", StringComparison.Ordinal));

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.FileSpaceId = null;
      ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2084", StringComparison.Ordinal));

      ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(null));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2082", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateMapToCreate()
    {
      var boundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      var csContent = "MDBUTVNDIFYxMC03MCAgICAgICAwICAgMDkvMDEvMjAyMCAxNToxOTExMzExMQ0KMTBUTVVudGl0bGVkIEpvYiAgICAxMjIyMTINCjc4VE0xMQ0KRDVUTSAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA0KRDhUTSAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgDQo2NFRNMzM2LjIwNjU1NTM3MTAwMDAtMTE1LjAyNjI2NzgxODAwMC4wMDAwMDAwMDAwMDAwMDExMTkuNzQ4NDM3ODk2OTAyMTkzLjk3OTQ3Njc1OTAwMC4wMDAwMDAwMDAwMDAwMDEuMDAwMDg2NzIzMDAwMDAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIA0KNjVUTTYzNzgxMzcuMDAwMDAxMDAyOTguMjU3MjIyOTMyODkwDQo0OVRNMzYzNzgxMzcuMDAwMDAwMDAyOTguMjU3MjIzNTYzMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMDAuMDAwMDAwMDAwMDAwMDAwLjAwMDAwMDAwMDAwMDAwMC4wMDAwMDAwMDAwMDAwMA0KNTBUTTExOTguNzk3MzM5MDkwNzAyNDkxLjQ4Mjg5MTU0MTgwMC4wMDI2NjA5MDkzMjE4MDAuMDAwMTM3MTYwMjc0MzAwLjAwMTkwNzk5NzAwMDAwMS4wMDAwMTMwMTMwMDAwMA0KQzhUTTRTQ1M5MDAgTG9jYWxpemF0aW9uICAgICAgICAgICAgIFNDUzkwMCBSZWNvcmQgICAgICAgICAgICAgICAgICAgRGF0dW0gZnJvbSBEYXRhIENvbGxlY3RvciAgICAgICANCg==";
      var csContentBase64 = "TURCVVRWTkRJRll4TUMwM01DQWdJQ0FnSUNBd0lDQWdNRGt2TURFdk1qQXlNQ0F4TlRveE9URXhNekV4TVEwS01UQlVUVlZ1ZEdsMGJHVmtJRXB2WWlBZ0lDQXhNakl5TVRJTkNqYzRWRTB4TVEwS1JEVlVUU0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lBMEtSRGhVVFNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdEUW8yTkZSTk16TTJMakl3TmpVMU5UTTNNVEF3TURBdE1URTFMakF5TmpJMk56Z3hPREF3TUM0d01EQXdNREF3TURBd01EQXdNREV4TVRrdU56UTRORE0zT0RrMk9UQXlNVGt6TGprM09UUTNOamMxT1RBd01DNHdNREF3TURBd01EQXdNREF3TURFdU1EQXdNRGcyTnpJek1EQXdNREFnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lBMEtOalZVVFRZek56Z3hNemN1TURBd01EQXhNREF5T1RndU1qVTNNakl5T1RNeU9Ea3dEUW8wT1ZSTk16WXpOemd4TXpjdU1EQXdNREF3TURBeU9UZ3VNalUzTWpJek5UWXpNREF3TUM0d01EQXdNREF3TURBd01EQXdNREF1TURBd01EQXdNREF3TURBd01EQXdMakF3TURBd01EQXdNREF3TURBd01DNHdNREF3TURBd01EQXdNREF3TURBdU1EQXdNREF3TURBd01EQXdNREF3TGpBd01EQXdNREF3TURBd01EQXdNQzR3TURBd01EQXdNREF3TURBd01BMEtOVEJVVFRFeE9UZ3VOemszTXpNNU1Ea3dOekF5TkRreExqUTRNamc1TVRVME1UZ3dNQzR3TURJMk5qQTVNRGt6TWpFNE1EQXVNREF3TVRNM01UWXdNamMwTXpBd0xqQXdNVGt3TnprNU56QXdNREF3TVM0d01EQXdNVE13TVRNd01EQXdNQTBLUXpoVVRUUlRRMU01TURBZ1RHOWpZV3hwZW1GMGFXOXVJQ0FnSUNBZ0lDQWdJQ0FnSUZORFV6a3dNQ0JTWldOdmNtUWdJQ0FnSUNBZ0lDQWdJQ0FnSUNBZ0lDQWdSR0YwZFcwZ1puSnZiU0JFWVhSaElFTnZiR3hsWTNSdmNpQWdJQ0FnSUNBTkNnPT0=";
      var projectValidation = new ProjectValidation
      {
       CustomerUid = new Guid("372854b8-64f8-4fd0-885f-d663503ffbca"),
       ProjectType = CwsProjectType.AcceptsTagFiles,
       ProjectName = "Beside Dimensions JeanieTest1",
       ProjectBoundaryWKT = boundaryString,
       UpdateType = ProjectUpdateType.Created,
       CoordinateSystemFileName = "myOne.dc",
       CoordinateSystemFileContent = System.Text.Encoding.ASCII.GetBytes(csContent)
       };

      var createProjectRequestModel = AutoMapperUtility.Automapper.Map<CreateProjectRequestModel>(projectValidation);

      Assert.Equal(TRNHelper.MakeTRN(projectValidation.CustomerUid, TRNHelper.TRN_ACCOUNT), createProjectRequestModel.TRN);
      Assert.Equal(projectValidation.CustomerUid.ToString(), createProjectRequestModel.AccountId);
      Assert.Equal(projectValidation.ProjectName, createProjectRequestModel.ProjectName);
      Assert.Equal(projectValidation.ProjectType, createProjectRequestModel.ProjectType);
      Assert.Null(createProjectRequestModel.Timezone);
      Assert.Equal(GeometryConversion.MapProjectBoundary(projectValidation.ProjectBoundaryWKT).type, createProjectRequestModel.Boundary.type);
      Assert.Equal(GeometryConversion.MapProjectBoundary(projectValidation.ProjectBoundaryWKT).coordinates.Count, createProjectRequestModel.Boundary.coordinates.Count);
      Assert.Equal(GeometryConversion.MapProjectBoundary(projectValidation.ProjectBoundaryWKT).coordinates.ToArray(), createProjectRequestModel.Boundary.coordinates.ToArray());
      Assert.Equal(projectValidation.CoordinateSystemFileName, createProjectRequestModel.CalibrationFileName);
      Assert.Equal(csContentBase64, createProjectRequestModel.CalibrationFileBase64Content);
      Assert.Equal(csContent, System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(createProjectRequestModel.CalibrationFileBase64Content)));
    }
  }
}

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
      Assert.Equal(csContent, createProjectRequestModel.CalibrationFileBase64Content);
    }
  }
}

using System;
using VSS.Common.Exceptions;
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
  }
}

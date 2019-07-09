using System;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests.Models
{
  public class ImportedFileBaseTests
  {
    [Theory]
    [InlineData(ImportedFileType.Alignment, true)]
    [InlineData(ImportedFileType.DesignSurface, true)]
    [InlineData(ImportedFileType.Linework, false)]
    [InlineData(ImportedFileType.MassHaulPlan, false)]
    [InlineData(ImportedFileType.MobileLinework, false)]
    [InlineData(ImportedFileType.ReferenceSurface, true)]
    [InlineData(ImportedFileType.SiteBoundary, false)]
    [InlineData(ImportedFileType.SurveyedSurface, true)]
    public void IsDesignFileType_returns_correct_value_For_ImportedFileType(ImportedFileType importedFileType, bool expectedResult)
    {
      var obj = new DeleteImportedFile(Guid.NewGuid(), importedFileType, null, Guid.NewGuid(), 0, 0, null);

      Assert.Equal(expectedResult, obj.IsDesignFileType);
    }
  }
}

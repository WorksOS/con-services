using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models.Extensions
{
  [TestClass]
  public class FileDataExtensionsTests
  {
    [TestMethod]
    [DataRow(ImportedFileType.DesignSurface, true)]
    [DataRow(ImportedFileType.SurveyedSurface, true)]
    [DataRow(ImportedFileType.ReferenceSurface, false)]
    [DataRow(ImportedFileType.Alignment, false)]
    [DataRow(ImportedFileType.Linework, false)]
    [DataRow(ImportedFileType.MassHaulPlan, false)]
    [DataRow(ImportedFileType.MobileLinework, false)]
    [DataRow(ImportedFileType.SiteBoundary, false)]
    public void Should_validate_correctly_When_given_an_imported_filetype(ImportedFileType importedFileType,
      bool expectedResult)
    {
      var fileData = new FileData { ImportedFileType = importedFileType };

      Assert.AreEqual(expectedResult, fileData.IsProfileSupportedFileType());
    }
  }
}
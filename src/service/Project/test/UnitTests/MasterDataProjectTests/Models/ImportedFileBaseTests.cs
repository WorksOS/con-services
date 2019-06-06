using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests.Models
{
  [TestClass]
  public class ImportedFileBaseTests
  {
    [TestMethod]
    [DataRow(ImportedFileType.Alignment, true)]
    [DataRow(ImportedFileType.DesignSurface, true)]
    [DataRow(ImportedFileType.Linework, false)]
    [DataRow(ImportedFileType.MassHaulPlan, false)]
    [DataRow(ImportedFileType.MobileLinework, false)]
    [DataRow(ImportedFileType.ReferenceSurface, true)]
    [DataRow(ImportedFileType.SiteBoundary, false)]
    [DataRow(ImportedFileType.SurveyedSurface, true)]
    public void IsDesignFileType_returns_correct_value_For_ImportedFileType(ImportedFileType importedFileType, bool expectedResult)
    {
      var obj = new DeleteImportedFile(Guid.NewGuid(), importedFileType, null, Guid.NewGuid(), 0, 0, null);

      Assert.AreEqual(expectedResult, obj.IsDesignFileType);
    }
  }
}

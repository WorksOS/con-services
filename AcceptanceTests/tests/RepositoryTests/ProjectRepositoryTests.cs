using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectRepositoryTests : TestControllerBase
  {
    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    public void ProjectSchemaExists_ImportedFileTable()
    {
      const string tableName = "ImportedFile";
      List<string> columnNames = new List<string>
      {
        "fk_ProjectUID",
        "ImportedFileUID",
        "ImportedFileID",
        "LegacyImportedFileID",
        "fk_CustomerUID",
        "fk_ImportedFileTypeID",
        "Name",
        "FileDescriptor",
        "FileCreatedUTC",
        "FileUpdatedUTC",
        "ImportedBy",
        "SurveyedUTC",
        "MinZoomLevel",
        "MaxZoomLevel",
        "fk_DXFUnitsTypeID",
        "IsDeleted",
        "LastActionedUTC",
        "InsertUTC",
        "UpdateUTC"
      };
      CheckSchema("_PROJECT", tableName, columnNames);
    }
  }
}

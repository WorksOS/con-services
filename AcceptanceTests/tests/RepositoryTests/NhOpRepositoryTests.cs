using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RepositoryTests
{
  [TestClass]
  public class NhOpRepositoryTests : TestControllerBase
  {
    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    [Ignore]
    public void NhOpSchemaExists_ImportedFileTable()
    {
      // todo make NH_OP.ImportedFile specific if we can
      const string tableName = "ImportedFile";
      List<string> columnNames = new List<string>
      {
        "fk_ProjectUID",
        "ImportedFileUID",
        "ImportedFileID",
        "fk_CustomerUID",
        "fk_ImportedFileTypeID",
        "Name",
        "FileDescriptor",
        "FileCreatedUTC",
        "FileUpdatedUTC",
        "ImportedBy",
        "SurveyedUTC",
        "fk_DXFUnitsTypeID",
        "IsDeleted",
        "IsActivated",
        "LastActionedUTC",
        "InsertUTC",
        "UpdateUTC"
      };
      CheckSchema("_NH_OP", tableName, columnNames);
    }
  }
}

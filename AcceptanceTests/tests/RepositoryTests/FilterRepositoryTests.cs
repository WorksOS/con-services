using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RepositoryTests
{
  [TestClass]
  public class FilterRepositoryTests : TestControllerBase
  {
    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    public void FilterSchemaExists_FilterTable()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID",
        "FilterUID",
        "fk_FilterTypeID",
        "fk_CustomerUID",
        "fk_ProjectUID",
        "UserID",
        "Name",
        "FilterJson",
        "IsDeleted",
        "LastActionedUTC",
        "InsertUTC",
        "UpdateUTC"
      };
      CheckSchema("_FILTER", tableName, columnNames);
    }
  }
}

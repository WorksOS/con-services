using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RepositoryTests
{
  [TestClass]
  public class FilterRepositoryTests : TestControllerBase
  {
    protected ILogger log;

    [TestInitialize]
    public void Init()
    {
      SetupDI();

      log = LoggerFactory.CreateLogger<FilterRepositoryTests>();
      Assert.IsNotNull(log, "log is null");
    }

    [TestMethod]
    public void FilterSchemaExists_FilterTable()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID",
        "FilterUID",
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

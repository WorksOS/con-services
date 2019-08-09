using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.Serilog.Extensions;

namespace RepositoryTests
{
  [TestClass]
  public class SchemaTests
  {
    public IServiceProvider serviceProvider;
    IConfigurationStore gc;

    [TestInitialize]
    public virtual void InitTest()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.TagFileAuth.WepApiTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IRepositoryFactory, RepositoryFactory>()
        .BuildServiceProvider();

      gc = serviceProvider.GetService<IConfigurationStore>();
    }

    [TestMethod]
    public void AssetSchemaExists()
    {
      const string tableName = "Asset";
      var columnNames = new List<string>
          {
            "ID", "AssetUID", "LegacyAssetID", "Name" , "MakeCode" , "SerialNumber", "Model", "ModelYear", "IconKey", "AssetType", "IsDeleted", "OwningCustomerUID", "EquipmentVIN", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerSchemaExists()
    {
      const string tableName = "Customer";
      var columnNames = new List<string>
          {
            "ID", "CustomerUID", "Name", "fk_CustomerTypeID", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void DeviceSchemaExists()
    {
      const string tableName = "Device";
      var columnNames = new List<string>
          {
            "ID", "DeviceUID", "DeviceSerialNumber", "DeviceType" , "DeviceState" , "DeregisteredUTC", "ModuleType", "MainboardSoftwareVersion", "RadioFirmwarePartNumber", "GatewayFirmwarePartNumber", "DataLinkType", "OwningCustomerUID", "LastActionedUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void AssetDeviceSchemaExists()
    {
      const string tableName = "AssetDevice";
      var columnNames = new List<string>
          {
            "ID", "fk_DeviceUID", "fk_AssetUID", "LastActionedUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    [Ignore("Temporary so old and new deployments can run side by side using a common database")]
    public void ProjectSchemaExists()
    {
      const string tableName = "Project";
      var columnNames = new List<string>
          {
            "ProjectUID", "LegacyProjectID", "Name", "Description", "fk_ProjectTypeID", "IsDeleted", "ProjectTimeZone", "LandfillTimeZone", "StartDate", "EndDate", "GeometryWKT", "LastActionedUTC", "InsertUTC", "UpdateUTC", "PolygonST", "CoordinateSystemFileName", "CoordinateSystemLastActionedUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerProjectSchemaExists()
    {
      const string tableName = "CustomerProject";
      var columnNames = new List<string>
          {
            "fk_CustomerUID", "fk_ProjectUID", "LegacyCustomerID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerTccOrgSchemaExists()
    {
      const string tableName = "CustomerTccOrg";
      var columnNames = new List<string>
          {
            "CustomerUID", "TCCOrgID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SubscriptionSchemaExists()
    {
      const string tableName = "Subscription";
      var columnNames = new List<string>
          {
            "ID", "SubscriptionUID", "fk_CustomerUID", "fk_ServiceTypeID", "StartDate", "EndDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectSubscriptionSchemaExists()
    {
      const string tableName = "ProjectSubscription";
      var columnNames = new List<string>
          {
            "fk_ProjectUID", "fk_SubscriptionUID", "EffectiveDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void AssetSubscriptionSchemaExists()
    {
      const string tableName = "AssetSubscription";
      var columnNames = new List<string>
          {
            "fk_AssetUID", "fk_SubscriptionUID", "EffectiveDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }


    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(gc.GetConnectionString("VSPDB")))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();
          Assert.IsNotNull(table, "Missing " + tableName + " table schema");
          Assert.AreEqual(tableName, table.TABLE_NAME, "Wrong table name");

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.IsNotNull(columns, "Missing " + tableName + " table columns");
          Assert.AreEqual(columnNames.Count, columns.Count, "Wrong number of " + tableName + " columns");
          foreach (var columnName in columnNames)
            Assert.IsNotNull(columns.Find(c => c.COLUMN_NAME == columnName), "Missing " + columnName + " column in " + tableName + " table");
        }
        finally
        {
          connection.Close();
        }
      }
    }

    private string GetQuery(string tableName, bool selectTable)
    {
      var what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = $"SELECT {what} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{gc.GetValueString("MYSQL_DATABASE_NAME")}' AND TABLE_NAME ='{tableName}'";
      return query;
    }
  }
}

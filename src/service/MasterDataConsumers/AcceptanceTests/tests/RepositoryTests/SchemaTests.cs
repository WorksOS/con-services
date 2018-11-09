using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using RepositoryTests.Internal;
using System.Collections.Generic;
using System.Linq;
using VSS.ConfigurationStore;

namespace RepositoryTests
{
  [TestClass]
  public class SchemaTests : TestControllerBase
  {
    IConfigurationStore gc;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      gc = ServiceProvider.GetService<IConfigurationStore>();
    }

    [TestMethod]
    public void AssetSchemaExists()
    {
      const string tableName = "Asset";
      List<string> columnNames = new List<string>
          {
            "ID", "AssetUID", "LegacyAssetID", "Name" , "MakeCode" , "SerialNumber", "Model", "ModelYear", "IconKey", "AssetType", "IsDeleted", "OwningCustomerUID", "EquipmentVIN", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerSchemaExists()
    {
      const string tableName = "Customer";
      List<string> columnNames = new List<string>
          {
            "ID", "CustomerUID", "Name", "fk_CustomerTypeID", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerUserSchemaExists()
    {
      const string tableName = "CustomerUser";
      List<string> columnNames = new List<string>
          {
            "fk_CustomerUID", "UserUID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }


    [TestMethod]
    public void DeviceSchemaExists()
    {
      const string tableName = "Device";
      List<string> columnNames = new List<string>
          {
            "ID", "DeviceUID", "DeviceSerialNumber", "DeviceType" , "DeviceState" , "DeregisteredUTC", "ModuleType", "MainboardSoftwareVersion", "RadioFirmwarePartNumber", "GatewayFirmwarePartNumber", "DataLinkType", "OwningCustomerUID", "LastActionedUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void AssetDeviceSchemaExists()
    {
      const string tableName = "AssetDevice";
      List<string> columnNames = new List<string>
          {
            "ID", "fk_DeviceUID", "fk_AssetUID", "LastActionedUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectSchemaExists()
    {
      const string tableName = "Project";
      List<string> columnNames = new List<string>
          {
            "ProjectUID", "LegacyProjectID", "Name", "Description", "fk_ProjectTypeID", "IsDeleted", "ProjectTimeZone", "LandfillTimeZone", "StartDate", "EndDate", "GeometryWKT", "LastActionedUTC", "InsertUTC", "UpdateUTC", "PolygonST", "CoordinateSystemFileName", "CoordinateSystemLastActionedUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerProjectSchemaExists()
    {
      const string tableName = "CustomerProject";
      List<string> columnNames = new List<string>
          {
            "fk_CustomerUID", "fk_ProjectUID", "LegacyCustomerID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectSettingsSchemaExists()
    {
      const string tableName = "ProjectSettings";
      List<string> columnNames = new List<string>
      {
        "fk_ProjectUID", "fk_ProjectSettingsTypeID", "Settings", "UserID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void CustomerTccOrgSchemaExists()
    {
      const string tableName = "CustomerTccOrg";
      List<string> columnNames = new List<string>
          {
            "CustomerUID", "TCCOrgID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ImportedFileSchemaExists()
    {
      const string tableName = "ImportedFile";
      List<string> columnNames = new List<string>
      {
        "fk_ProjectUID", "ImportedFileUID", "ImportedFileID", "LegacyImportedFileID", "fk_CustomerUID", "fk_ImportedFileTypeID", "Name", "FileDescriptor", "FileCreatedUTC", "FileUpdatedUTC", "ImportedBy", "SurveyedUTC", "fk_DXFUnitsTypeID", "MinZoomLevel", "MaxZoomLevel", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ImportedFileHistorySchemaExists()
    {
      const string tableName = "ImportedFileHistory";
      List<string> columnNames = new List<string>
      {
        "ID", "fk_ImportedFileUID", "FileCreatedUTC", "FileUpdatedUTC", "ImportedBy", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SubscriptionSchemaExists()
    {
      const string tableName = "Subscription";
      List<string> columnNames = new List<string>
          {
            "ID", "SubscriptionUID", "fk_CustomerUID", "fk_ServiceTypeID", "StartDate", "EndDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectSubscriptionSchemaExists()
    {
      const string tableName = "ProjectSubscription";
      List<string> columnNames = new List<string>
          {
            "fk_ProjectUID", "fk_SubscriptionUID", "EffectiveDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void AssetSubscriptionSchemaExists()
    {
      const string tableName = "AssetSubscription";
      List<string> columnNames = new List<string>
          {
            "fk_AssetUID", "fk_SubscriptionUID", "EffectiveDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void GeofenceSchemaExists()
    {
      const string tableName = "Geofence";
      List<string> columnNames = new List<string>
          {
            "ID", "GeofenceUID", "Name", "fk_GeofenceTypeID", "GeometryWKT", "FillColor", "IsTransparent", "IsDeleted", "LastActionedUTC", "Description", "AreaSqMeters", "fk_CustomerUID", "UserUID", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectGeofenceSchemaExists()
    {
      const string tableName = "ProjectGeofence";
      List<string> columnNames = new List<string>
          {
            "fk_ProjectUID", "fk_GeofenceUID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void FilterSchemaExists()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID", "FilterUID", "fk_CustomerUID", "fk_ProjectUID", "UserID" , "Name" , "FilterJson", "fk_FilterTypeID", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
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
      string what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = string.Format("SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, gc.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.Project.Service.Utils;

namespace RepositoryTests
{
  [TestClass]
  public class SchemaTests
  {
    GenericConfiguration gc = new GenericConfiguration();

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
    public void ProjectSchemaExists()
    {
      const string tableName = "Project";
      List<string> columnNames = new List<string>
          {
            "ID", "ProjectUID", "LegacyProjectID", "Name", "fk_ProjectTypeID", "IsDeleted", "ProjectTimeZone", "LandfillTimeZone", "StartDate", "EndDate", "LastActionedUTC", "InsertUTC", "UpdateUTC"
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
    public void GeofenceSchemaExists()
    {
      const string tableName = "Geofence";
      List<string> columnNames = new List<string>
          {
            "ID", "GeofenceUID", "Name", "fk_GeofenceTypeID", "GeometryWKT", "FillColor", "IsTransparent", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
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

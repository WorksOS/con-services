extern alias MySqlDataAlias;
using Dapper;
using RepositoryTests.Internal;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RepositoryTests
{
  public class SchemaTests : TestControllerBase
  {
    public SchemaTests()
    {
      SetupLogging();
    }

    [Fact]
    public void DeviceSchemaExists()
    {
      const string tableName = "Device";
      var columnNames = new List<string>
          {
            "DeviceUID", "ShortRaptorAssetID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void ProjectSchemaExists()
    {
      const string tableName = "Project";
      var columnNames = new List<string>
          {
            "ProjectUID", "CustomerUID", "ShortRaptorProjectID", "Name", "fk_ProjectTypeID", "ProjectTimeZone", "ProjectTimeZoneIana", "Boundary", "CoordinateSystemFileName", "CoordinateSystemLastActionedUTC", "IsArchived", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void ProjectHistorySchemaExists()
    {
      const string tableName = "ProjectHistory";
      var columnNames = new List<string>
          {
            "ProjectUID", "CustomerUID", "ShortRaptorProjectID", "Name", "fk_ProjectTypeID", "ProjectTimeZone", "ProjectTimeZoneIana", "Boundary", "CoordinateSystemFileName", "CoordinateSystemLastActionedUTC", "IsArchived", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void ProjectSettingsSchemaExists()
    {
      const string tableName = "ProjectSettings";
      var columnNames = new List<string>
      {
        "fk_ProjectUID", "fk_ProjectSettingsTypeID", "Settings", "UserID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void ImportedFileSchemaExists()
    {
      const string tableName = "ImportedFile";
      var columnNames = new List<string>
      {
        "fk_ProjectUID", "ImportedFileUID", "ImportedFileID", "LegacyImportedFileID", "fk_CustomerUID", "fk_ImportedFileTypeID", "Name",
        "FileDescriptor", "FileCreatedUTC", "FileUpdatedUTC", "ImportedBy", "SurveyedUTC", "fk_DXFUnitsTypeID", "MinZoomLevel", "MaxZoomLevel",
        "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC", "Offset", "fk_ReferenceImportedFileUID"
      };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void ImportedFileHistorySchemaExists()
    {
      const string tableName = "ImportedFileHistory";
      var columnNames = new List<string>
      {
        "ID", "fk_ImportedFileUID", "FileCreatedUTC", "FileUpdatedUTC", "ImportedBy", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void GeofenceSchemaExists()
    {
      const string tableName = "Geofence";
      var columnNames = new List<string>
          {
            "ID", "GeofenceUID", "Name", "fk_GeofenceTypeID", "PolygonST", "FillColor", "IsTransparent", "IsDeleted", "Description", "AreaSqMeters", "fk_CustomerUID", "UserUID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [Fact]
    public void ProjectGeofenceSchemaExists()
    {
      const string tableName = "ProjectGeofence";
      var columnNames = new List<string>
          {
            "ID", "fk_ProjectUID", "fk_GeofenceUID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlDataAlias.MySql.Data.MySqlClient.MySqlConnection(configStore.GetConnectionString("VSPDB")))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();
          Assert.NotNull(table);
          Assert.Equal(tableName, table.TABLE_NAME);

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.NotNull(columns);
          Assert.Equal(columnNames.Count, columns.Count());
          foreach (var columnName in columnNames)
            Assert.NotNull(columns.Find(c => c.COLUMN_NAME == columnName));
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
        what, configStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }
  }
}

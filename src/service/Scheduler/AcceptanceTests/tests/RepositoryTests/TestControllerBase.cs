using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using Xunit;

namespace RepositoryTests
{
  public class TestControllerBase
  {
    protected ILoggerFactory LoggerFactory;
    protected IConfigurationStore ConfigStore;

    private IServiceProvider _serviceProvider;

    public void SetupDI()
    {
      var logger = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Scheduler.Respository.AcceptanceTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(logger)
                       .AddSingleton<IConfigurationStore, GenericConfiguration>();

      _serviceProvider = serviceCollection.BuildServiceProvider();

      ConfigStore = _serviceProvider.GetRequiredService<IConfigurationStore>();

      LoggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

      Assert.NotNull(_serviceProvider.GetService<IConfigurationStore>());
      Assert.NotNull(_serviceProvider.GetService<ILoggerFactory>());
    }


    #region schema

    public void CheckSchema(string tableName, List<string> columnNames)
    {
      string connectionString = ConfigStore.GetConnectionString("VSPDB");
      Console.WriteLine($"CheckSchema() connectionString: {connectionString}");

      using (var connection = new MySqlConnection(connectionString))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();

          Assert.NotNull(table);
          Assert.Equal(tableName, (string)table.TABLE_NAME);

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.NotNull(columns);
          Assert.Equal(columnNames.Count, columns.Count);
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
      var query = string.Format(
        "SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, ConfigStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }

    #endregion schema
  }
}

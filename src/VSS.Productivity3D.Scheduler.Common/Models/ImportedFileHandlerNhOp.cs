using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileHandlerNhOp<T> : IImportedFileHandler<T>
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private string _dbConnectionString;
    private List<NhOpImportedFile> _members;
 
    public ImportedFileHandlerNhOp(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileHandlerNhOp<T>>();
      _dbConnectionString = ConnectionUtils.GetConnectionStringMsSql(_configStore, _log, "_NH_OP"); ;
      _members = new List<NhOpImportedFile>();

      _log.LogDebug($"ImportedFileHandlerNhOp.ImportedFileHandlerNhOp() _configStore {JsonConvert.SerializeObject(_configStore)}");
      Console.WriteLine($"ImportedFileHandlerNhOp.ImportedFileHandlerNhOp() _configStore {JsonConvert.SerializeObject(_configStore)}");
    }


    public int Read()
    {
      SqlConnection dbConnection = new SqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.Read: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.Read: open DB exeception {ex.Message}");
        throw;
      }

      // todo :
      // Name: strip off date for e.g. StockPile_2014-05-21T210701Z.TTM
      // FileDescription: form this from FileSpaceID, CustomerUID, ProjectUID

      // missing here are:
      // SourcePath, SourceFilespaceID - ignore
      // fk_ReferenceImportedFileID, Offset
      // fk_MassHaulPlanID
      // MinZoom, MaxZoom
      // MinLat...
      // IsNotifyUser
      string selectCommand = @"SELECT 
              p.ID AS LegacyProjectId, CAST(p.ProjectUID AS varchar(100)) AS ProjectUid,
              c.ID AS LegacyCustomerId, CAST(c.CustomerUID AS varchar(100)) AS CustomerUid,
              fk_ImportedFileTypeID AS ImportedFileType, iff.Name,               
              SurveyedUtc, fk_DXFUnitsTypeID AS DxfUnitsType,
              FileCreatedUtc, FileUpdatedUtc, u.EmailContact AS ImportedBy,
              iff.InsertUTC AS LastActionedUtc
            FROM ImportedFile iff
              INNER JOIN Customer c ON c.ID = fk_CustomerID
              INNER JOIN Project p ON p.ID = fk_ProjectID 
              OUTER APPLY (SELECT TOP 1 CreateUTC AS FileUpdatedUtc, fk_UserID FROM ImportedFileHistory WHERE fk_ImportedFileID = iff.ID ORDER BY InsertUTC desc) ifhLast
              OUTER APPLY (SELECT TOP 1 CreateUTC AS FileCreatedUtc FROM ImportedFileHistory WHERE fk_ImportedFileID = iff.ID ORDER BY InsertUTC asc) ifhFirst
              LEFT OUTER JOIN [User] u on u.id = ifhLast.fk_UserID
            WHERE fk_ImportedFileTypeID = 2";
      List<NhOpImportedFile> response;
      try
      {
        response = dbConnection.Query<NhOpImportedFile>(selectCommand).ToList();
        Console.WriteLine($"ImportedFileHandlerNhOp.Read: selectCommand {selectCommand} response {JsonConvert.SerializeObject(response)}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.Read:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.Read:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerNhOp.Read:  dbConnection.Close");
      }
      _members.AddRange(response);

      return _members.Count;
    }

    public void EmptyList()
    {
      _members.Clear();
    }

    public List<T> List() 
    {
      return _members as List<T>;
    }

    public int Create(List<T> memberList)
    {
      SqlConnection dbConnection = new SqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.Create: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.Create: open DB exeception {ex.Message}");
        throw;
      }

      var insertImportedFileCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_CustomerID, fk_ProjectID, Name, fk_ImportedFileTypeID, SurveyedUTC, fk_DXFUnitsTypeID) " +
        "  VALUES " +
        "    (@CustomerUid, @LegacyProjectId, @Name, @ImportedFileType, @SurveyedUtc, @DxfUnitsType)");

      // todo don't know the legacy UserID
      var insertImportedFileHistoryeCommand = string.Format(
        "INSERT ImportedFileHistory " +
        "    (fk_ImportedFileID, InsertUTC, CreateUTC, fk_UserID) " +
        "  VALUES " +
        "    (@returnedImportedFileID, @FileUpdatedUtc, @FileCreatedUtc, 0)");

      int countInserted = 0;
      try
      {
        // todo do multiple insert?
        foreach (var member in memberList)
        {
          countInserted += dbConnection.Execute(insertImportedFileCommand, member);
          // todo get the just inserted ImportedFile ID for the history
          long @returnedImportedFileID = 0;
          countInserted += dbConnection.Execute(insertImportedFileHistoryeCommand, member);
          Console.WriteLine(
            $"ImportedFileHandlerNhOp.Create: member {JsonConvert.SerializeObject(member)} countInsertedSoFar {JsonConvert.SerializeObject(countInserted)}");
        }
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.Create:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.Create:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerNhOp.Create:  dbConnection.Close");
      }

      InsertHistory(memberList);
      return countInserted;
    }

    public int InsertHistory(List<T> memberList)
    {
      SqlConnection dbConnection = new SqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.Create: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.Create: open DB exeception {ex.Message}");
        throw;
      }

      var insertImportedFileCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_CustomerID, fk_ProjectID, Name, fk_ImportedFileTypeID, SurveyedUTC, fk_DXFUnitsTypeID) " +
        "  VALUES " +
        "    (@CustomerUid, @LegacyProjectId, @Name, @ImportedFileType, @SurveyedUtc, @DxfUnitsType)");

//// todo don't know the legacy UserID
//      var insertImportedFileHistoryeCommand = string.Format(
//        "INSERT ImportedFileHistory " +
//        "    (fk_ImportedFileID, InsertUTC, CreateUTC, fk_UserID) " +
//        "  VALUES " +
//        "    (@returnedImportedFileID, @FileUpdatedUtc, @FileCreatedUtc, 0)");

      int countInserted = 0;
      try
      {
        // todo do multiple insert?
        foreach (var member in memberList)
        {
          countInserted += dbConnection.Execute(insertImportedFileCommand, member);
          //// todo get the just inserted ImportedFile ID for the history
          //long @returnedImportedFileID = 0;
          //countInserted += dbConnection.Execute(insertImportedFileHistoryeCommand, member);
          Console.WriteLine(
            $"ImportedFileHandlerNhOp.Create: member {JsonConvert.SerializeObject(member)} countInsertedSoFar {JsonConvert.SerializeObject(countInserted)}");
        }
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.Create:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.Create:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerNhOp.Create:  dbConnection.Close");
      }
      return countInserted;
    }

    /// <summary>
    ///  the only thing 'updateable' in a CG ImportFile is to add another history row
    /// </summary>
    /// <param name="memberList"></param>
    /// <returns></returns>
    public int Update(List<T> memberList)
    {
      return InsertHistory(memberList);
    }

    public int Delete(List<T> memberList)
    {
      SqlConnection dbConnection = new SqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Delete: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Delete: open DB exeception {ex.Message}");
        throw;
      }

      var deleteCommand =
        @"DELETE ImportedFile                
                WHERE ImportedFileID = @ImportedFileId";
      int countUpdated = 0;
      try
      {
        // todo do multiple update?
        foreach (var member in memberList)
        {
          countUpdated += dbConnection.Execute(deleteCommand, member);
          Console.WriteLine(
            $"ImportedFileHandlerProject.Update: member {JsonConvert.SerializeObject(member)} countUpdatedSoFar {JsonConvert.SerializeObject(countUpdated)}");
        }
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Delete:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Delete:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerProject.Delete:  dbConnection.Close");
      }

      return countUpdated;
    }

    public bool IsImportedFileDeleted(T member)
    {
      // todo how can we identify an Imported file from it's history when it doesn't save its ImportedFile.ID 
      // by FileCreatedUtc?????
      throw new NotImplementedException();
    }

  }
}

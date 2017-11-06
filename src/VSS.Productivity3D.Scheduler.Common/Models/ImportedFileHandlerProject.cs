using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileHandlerProject<T> : IImportedFileHandler<T>
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private string _dbConnectionString;
    private List<ProjectImportedFile> _members;


    public ImportedFileHandlerProject(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileHandlerProject<T>>();
      _dbConnectionString = ConnectionUtils.GetConnectionStringMySql(_configStore, _log, "_Project");
      _members = new List<ProjectImportedFile>();

      _log.LogDebug($"ImportedFileHandlerProject.ImportedFileHandlerProject() _configStore {JsonConvert.SerializeObject(_configStore)}");
      Console.WriteLine($"ImportedFileHandlerProject.ImportedFileHandlerProject() _configStore {JsonConvert.SerializeObject(_configStore)}");
    }

    public int Read()
    {
      MySqlConnection dbConnection = new MySqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Read: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Read: open DB exeception {ex.Message}");
        throw;
      }

      string selectCommand = 
        @"SELECT 
              iff.fk_ProjectUID as ProjectUID, iff.ImportedFileUID, iff.ImportedFileID, iff.fk_CustomerUID as CustomerUID,
              iff.fk_ImportedFileTypeID as ImportedFileType, iff.Name, 
              iff.FileDescriptor, iff.FileCreatedUTC, iff.FileUpdatedUTC, iff.ImportedBy, iff.SurveyedUTC, iff.fk_DXFUnitsTypeID AS DxfUnitsType, 
              iff.IsDeleted, iff.IsActivated, iff.LastActionedUTC,
			        p.LegacyProjectID, cp.LegacyCustomerID
            FROM ImportedFile iff
				      INNER JOIN Project p ON p.ProjectUID = iff.fk_ProjectUID
              INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
            WHERE fk_ImportedFileTypeID = 2";

      List<ProjectImportedFile> response;
      try
      {
        response = dbConnection.Query<ProjectImportedFile>(selectCommand).ToList();
        Console.WriteLine($"ImportedFileHandlerProject.Read: selectCommand {selectCommand} response {JsonConvert.SerializeObject(response)}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Read:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Read:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerProject.Read:  dbConnection.Close");
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
      MySqlConnection dbConnection = new MySqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Create: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Create: open DB exeception {ex.Message}");
        throw;
      }

      var insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID, IsDeleted, IsActivated, LastActionedUTC) " +
        "  VALUES " +
        "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc,@DxfUnitsType, 0, 1, @LastActionedUtc)");
      int countInserted = 0;
      try
      {
        // todo do multiple insert?
        foreach (var member in memberList)
        {
          countInserted += dbConnection.Execute(insertCommand, member);
          Console.WriteLine(
            $"ImportedFileHandlerProject.Create: member {JsonConvert.SerializeObject(member)} countInsertedSoFar {JsonConvert.SerializeObject(countInserted)}");
        }
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Create:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Create:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerProject.Create:  dbConnection.Close");
      }

      return countInserted;
    }

    public int Update(List<T> memberList)
    {
      MySqlConnection dbConnection = new MySqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Update: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Update: open DB exeception {ex.Message}");
        throw;
      }

      var updateCommand =
          @"UPDATE ImportedFile
                SET 
                  FileDescriptor = @fileDescriptor,
                  FileCreatedUTC = @fileCreatedUtc,
                  FileUpdatedUTC = @fileUpdatedUtc,
                  ImportedBy = @importedBy, 
                  SurveyedUTC = @surveyedUTC,
                  LastActionedUTC = @LastActionedUTC,
                  IsActivated = @IsActivated,
                  IsDeleted = @IsDeleted
                WHERE ImportedFileUID = @ImportedFileUid";
      int countUpdated = 0;
      try
      {
        // todo do multiple update?
        foreach (var member in memberList)
        {
          countUpdated += dbConnection.Execute(updateCommand, member);
          Console.WriteLine(
            $"ImportedFileHandlerProject.Update: member {JsonConvert.SerializeObject(member)} countUpdatedSoFar {JsonConvert.SerializeObject(countUpdated)}");
        }
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.Update:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.Update:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerProject.Update:  dbConnection.Close");
      }

      return countUpdated;
    }

    public int Delete(List<T> memberList)
    {
      _log.LogError($"ImportedFileHandlerProject.Delete: ");
      Console.WriteLine($"ImportedFileHandlerProject.Delete: ");

      return Update(memberList);
    }
    
  }
}

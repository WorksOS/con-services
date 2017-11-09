using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileRepoProject<T> : IImportedFileRepo<T> where T : ImportedFileProject
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private string _dbConnectionString;
    private MySqlConnection _dbConnection;

    public ImportedFileRepoProject(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileRepoProject<T>>();
      _dbConnectionString = ConnectionUtils.GetConnectionStringMySql(_configStore, _log, "_Project");
      _dbConnection = new MySqlConnection(_dbConnectionString);
    }

    public List<T> Read()
    {
      var members = new List<ImportedFileProject>();
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoProject.Read: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoProject.Read: open DB exeception {ex.Message}");
        throw;
      }

      string selectCommand =
        @"SELECT 
              p.LegacyProjectID, cp.LegacyCustomerID, 
              iff.fk_ProjectUID as ProjectUID, iff.ImportedFileUID, iff.ImportedFileID, 
              iff.LegacyImportedFileId, iff.fk_CustomerUID as CustomerUID,
              iff.fk_ImportedFileTypeID as ImportedFileType, iff.Name, 
              iff.FileDescriptor, iff.FileCreatedUTC, iff.FileUpdatedUTC, iff.ImportedBy, 
              iff.IsDeleted, iff.IsActivated, 
              iff.SurveyedUTC, iff.fk_DXFUnitsTypeID AS DxfUnitsType, 
              iff.LastActionedUTC			        
            FROM ImportedFile iff
				      INNER JOIN Project p ON p.ProjectUID = iff.fk_ProjectUID
              INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
            WHERE fk_ImportedFileTypeID = 2";

      List<ImportedFileProject> response;
      try
      {
        response = _dbConnection.Query<ImportedFileProject>(selectCommand).ToList();
        _log.LogTrace($"ImportedFileRepoProject.Read: responseCount {response.Count}");
        Console.WriteLine($"ImportedFileRepoProject.Read: responseCount {response.Count}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoProject.Read:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoProject.Read:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }
      members.AddRange(response);

      return members as List<T>;
    }

    public long Create(T member)
    {
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoProject.Create: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoProject.Create: open DB exeception {ex.Message}");
        throw;
      }

      var insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_ProjectUID, ImportedFileUID, LegacyImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID, IsDeleted, IsActivated, LastActionedUTC) " +
        "  VALUES " +
        "    (@ProjectUid, @ImportedFileUid, @LegacyImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc,@DxfUnitsType, 0, 1, @LastActionedUtc)");
      int countInserted = 0;
      try
      {
        countInserted += _dbConnection.Execute(insertCommand, member);
        _log.LogTrace($"ImportedFileRepoProject.Create: countInserted {countInserted}");
        Console.WriteLine($"ImportedFileRepoProject.Create: countInserted {countInserted}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoProject.Create:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoProject.Create:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }

      return countInserted;
    }

    public int Update(T member)
    {
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoProject.Update: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoProject.Update: open DB exeception {ex.Message}");
        throw;
      }

      var updateCommand =
        @"UPDATE ImportedFile
                SET 
                  LegacyImportedFileId = @LegacyImportedFileId,
                  FileCreatedUTC = @fileCreatedUtc,
                  FileUpdatedUTC = @fileUpdatedUtc,
                  LastActionedUTC = @LastActionedUTC,
                  IsDeleted = @IsDeleted
                WHERE ImportedFileUID = @ImportedFileUid";
      int countUpdated = 0;
      try
      {
        countUpdated += _dbConnection.Execute(updateCommand, member);
        _log.LogTrace($"ImportedFileRepoProject.Update: countUpdated {countUpdated}");
        Console.WriteLine($"ImportedFileRepoProject.Update: countUpdated {countUpdated}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoProject.Update:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoProject.Update:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }

      return countUpdated;
    }

    public int Delete(T member)
    {
      _log.LogTrace($"ImportedFileRepoProject.Delete: ");
      Console.WriteLine($"ImportedFileRepoProject.Delete: ");

      member.IsDeleted = true;
      return Update(member);
    }

  }
}

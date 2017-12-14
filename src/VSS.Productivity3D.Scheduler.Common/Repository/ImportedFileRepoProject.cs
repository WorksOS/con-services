using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace VSS.Productivity3D.Scheduler.Common.Repository
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
      var startUtc = DateTime.UtcNow;
      var members = new List<ImportedFileProject>();
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Read: open DB exeception {ex.Message}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
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
              INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID";

      List<ImportedFileProject> response;
      try
      {
        response = _dbConnection.Query<ImportedFileProject>(selectCommand).ToList();
        _log.LogTrace($"ImportedFileRepoProject.Read: responseCount {response.Count}");
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Read: execute DB exeception {ex.Message}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
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
      var startUtc = DateTime.UtcNow;
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Create: open DB exeception {ex.Message}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
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
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Create: execute DB exeception {ex.Message}. VSSProjectToCreate: {JsonConvert.SerializeObject(member)}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
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
      var startUtc = DateTime.UtcNow;
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Update: open DB exeception {ex.Message}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
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
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Update: execute DB exeception {ex.Message}. VSSProjectToUpdate: {JsonConvert.SerializeObject(member)}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, _log, newRelicAttributes);
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

      member.IsDeleted = true;
      return Update(member);
    }

  }
}

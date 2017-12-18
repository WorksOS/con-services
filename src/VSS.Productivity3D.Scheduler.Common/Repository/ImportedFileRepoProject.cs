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
    private readonly IConfigurationStore _configStore;
    private readonly ILogger _log;
    private readonly string _dbConnectionString;
    private readonly MySqlConnection _dbConnection;
    private readonly List<CustomerProject> _customerProjectList;

    public ImportedFileRepoProject(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileRepoProject<T>>();
      _dbConnectionString = ConnectionUtils.GetConnectionStringMySql(_configStore, _log, "_Project");
      _dbConnection = new MySqlConnection(_dbConnectionString);
      _customerProjectList = new List<CustomerProject>();
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
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, _log, newRelicAttributes);
        throw;
      }

      string selectImportedFilesCommand =
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

      string selectCustomerProjectCommand =
        @"SELECT 
              LegacyCustomerID, fk_CustomerUID as CustomerUID,
              LegacyProjectID, ProjectUID              
            FROM Project 
              INNER JOIN CustomerProject ON fk_ProjectUID = ProjectUID";

      try
      {
        var responseImportedFiles = _dbConnection.Query<ImportedFileProject>(selectImportedFilesCommand).ToList();
        members.AddRange(responseImportedFiles);
        _log.LogTrace($"ImportedFileRepoProject.Read: responseImportedFiles {responseImportedFiles.Count}");

        var responseCustomerProject = _dbConnection.Query<CustomerProject>(selectCustomerProjectCommand).ToList();
        _customerProjectList.AddRange(responseCustomerProject);
        _log.LogTrace($"ImportedFileRepoProject.Read: responseCustomerProject {responseCustomerProject.Count}");
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.Read: execute DB exeception {ex.Message}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, _log, newRelicAttributes);

        // throw on system issues, not business rule failure
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }

      return members as List<T>;
    }

    public bool ProjectAndCustomerExist(string customerUid, string projectUid)
    {
      return _customerProjectList
        .Any(x => (String.Compare(x.CustomerUid, customerUid, StringComparison.OrdinalIgnoreCase) == 0) 
            && (String.Compare(x.ProjectUid, projectUid, StringComparison.OrdinalIgnoreCase) == 0));
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
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, _log, newRelicAttributes);
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
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, _log, newRelicAttributes);
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
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, _log, newRelicAttributes);
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
        NewRelicUtils.NotifyNewRelic("ImportedFileRepoNhOp", "Error", startUtc, _log, newRelicAttributes);
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

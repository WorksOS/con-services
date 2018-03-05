﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using CustomerProject = VSS.Productivity3D.Scheduler.Common.Models.CustomerProject;

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

    public List<T> Read(bool processSurveyedSurfaceType)
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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
        throw;
      }

      string selectImportedFilesCommand =
        @"SELECT 
              p.LegacyProjectID, cp.LegacyCustomerID, 
              iff.fk_ProjectUID as ProjectUID, iff.ImportedFileUID, iff.ImportedFileID, 
              iff.LegacyImportedFileId, iff.fk_CustomerUID as CustomerUID,
              iff.fk_ImportedFileTypeID as ImportedFileType, iff.Name, 
              iff.FileDescriptor, iff.FileCreatedUTC, iff.FileUpdatedUTC, iff.ImportedBy, 
              iff.IsDeleted, 
              iff.SurveyedUTC, iff.fk_DXFUnitsTypeID AS DxfUnitsType, 
              iff.LastActionedUTC			        
            FROM ImportedFile iff
				      INNER JOIN Project p ON p.ProjectUID = iff.fk_ProjectUID
              INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID";
      selectImportedFilesCommand += string.Format($"   WHERE fk_ImportedFileTypeID {(processSurveyedSurfaceType ? "= 2" : "!= 2")}");

      string selectImportedFilesHistoryCommand =
        @"SELECT 
              ImportedFileUID, ifh.FileCreatedUTC, ifh.FileUpdatedUTC, ifh.ImportedBy	        
            FROM ImportedFile iff
				      INNER JOIN ImportedFileHistory ifh ON ifh.fk_ImportedFileUID = iff.ImportedFileUID";
      selectImportedFilesHistoryCommand += string.Format($"   WHERE fk_ImportedFileTypeID {(processSurveyedSurfaceType ? "= 2" : "!= 2")}");
      selectImportedFilesHistoryCommand += string.Format("   ORDER BY ImportedFileUID, ifh.FileCreatedUTC, ifh.FileUpdatedUTC");

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

        var responseImportedFileHistory = _dbConnection.Query<ImportedFileHistoryItem>(selectImportedFilesHistoryCommand).ToList();
        members.ForEach(y => y.ImportedFileHistory = (new ImportedFileHistory(responseImportedFileHistory.Where(h => h.ImportedFileUid == y.ImportedFileUid).ToList())));

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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);

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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
        throw;
      }

      var insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_ProjectUID, ImportedFileUID, LegacyImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID, IsDeleted, LastActionedUTC) " +
        "  VALUES " +
        "    (@ProjectUid, @ImportedFileUid, @LegacyImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc,@DxfUnitsType, 0, @LastActionedUtc)");
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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
      }
      finally
      {
        _dbConnection.Close();
      }

      if (countInserted > 0)
      {
        CreateHistory(member);
      }

      return countInserted;
    }

    public int CreateHistory(T member)
    {
      var startUtc = DateTime.UtcNow;
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.CreateHistory: open DB exeception {ex.Message}.";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
        throw;
      }

      var importedFileHistoryCommand = string.Format(
        "INSERT ImportedFileHistory " +
        "    (fk_ImportedFileUID, FileCreatedUtc, FileUpdatedUtc, ImportedBy) " +
        " VALUES " +
        " (@ImportedFileUid, @FileCreatedUtc, @FileUpdatedUtc, @ImportedBy)");

      int countInserted = 0;
      try
      {
        countInserted += _dbConnection.Execute(importedFileHistoryCommand,
          new { member.ImportedFileUid, member.FileCreatedUtc, member.FileUpdatedUtc, member.ImportedBy });
        _log.LogTrace($"ImportedFileRepoProject.CreateHistory: countInserted {countInserted}");
      }
      catch (Exception ex)
      {
        var message = $"ImportedFileRepoProject.CreateHistory:  execute exeception {ex.Message} projectHistoryToCreate: {JsonConvert.SerializeObject(member)}";
        var newRelicAttributes = new Dictionary<string, object>
        {
          {"message",message}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, _log, newRelicAttributes);
      }
      finally
      {
        _dbConnection.Close();
      }

      if (countUpdated > 0)
      {
        // as per Dmitry: don't bother backfilling entire history
        CreateHistory(member);
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

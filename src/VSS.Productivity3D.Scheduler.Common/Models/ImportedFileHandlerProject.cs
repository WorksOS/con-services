using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileHandlerProject<T> : IImportedFileHandler<T>
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private List<ImportedFile> _members = null;
    private List<ImportedFile> _insertMembers = null;
    private List<ImportedFile> _updateMembers = null;
    private string _dbConnectionString;

    public ImportedFileHandlerProject(IConfigurationStore configStore, ILoggerFactory logger,
      string dbConnectionString)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileHandlerProject<T>>();
      _members = new List<ImportedFile>();
      _insertMembers = new List<ImportedFile>();
      _updateMembers = new List<ImportedFile>();
      _dbConnectionString = dbConnectionString;

      _log.LogDebug(
        $"ImportedFileHandlerProject.ImportedFileHandlerProject() _configStore {JsonConvert.SerializeObject(_configStore)}");
      Console.WriteLine(
        $"ImportedFileHandlerProject.ImportedFileHandlerProject() _configStore {JsonConvert.SerializeObject(_configStore)}");
    }

    /// <summary>
    /// ReadFromDb from NGen Project.ImportedFile table
    ///   May initially be limited to SurveyedSurface type
    /// </summary>
    public int ReadFromDb()
    {
      // todo async
      // todo use dbrepo
      MySqlConnection dbConnection = new MySqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.ReadFromDb: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.ReadFromDb: open DB exeception {ex.Message}");
        throw;
      }

      string selectCommand = @"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID as CustomerUID,
              fk_ImportedFileTypeID as ImportedFileType, Name, 
              FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted, IsActivated,
              LastActionedUTC
            FROM ImportedFile
            WHERE fk_ImportedFileTypeID = 2";
      List<ImportedFile> response;
      try
      {
        response = dbConnection.Query<ImportedFile>(selectCommand).ToList();
        Console.WriteLine($"ImportedFileHandlerProject.ReadFromDb: selectCommand {selectCommand} response {JsonConvert.SerializeObject(response)}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.ReadFromDb:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.ReadFromDb:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerProject.ReadFromDb:  dbConnection.Close");
      }
      _members.AddRange(response);

      return _members.Count;
    }

    public void EmptyList()
    {
      _members.Clear();
      _insertMembers.Clear();
      _updateMembers.Clear();
    }

    public List<T> List() 
    {
      return _members as List<T>;
    }

    /// <summary>
    /// Merge a list from e.g. NHOP.ImportedFile table into existing _members
    /// i.e. resolving differences and removing duplicates to come up with a _members list with outstanding actions
    /// </summary>
    public int Merge( List<NhOpImportedFile> otherSourceMembers )
    {
      // todo use AutoMapper
      // todo how to match - up?
      foreach (var nhOpImportedFile in otherSourceMembers)
      {
        if (nhOpImportedFile.ImportedFileType == ImportedFileType.SurveyedSurface)
        {
          var importedFileProject = new ImportedFile
          {
            ProjectUid = nhOpImportedFile.ProjectUid,
            CustomerUid = nhOpImportedFile.CustomerUid,
            ImportedFileType = nhOpImportedFile.ImportedFileType,
            // todo DXFUnitsType not avail in NG yet
            Name = nhOpImportedFile.Name, // todo strip off date
            FileDescriptor = "todo",
            SurveyedUtc = nhOpImportedFile.SurveyedUtc,
            FileCreatedUtc = nhOpImportedFile.FileCreatedUtc, 
            FileUpdatedUtc = nhOpImportedFile.FileUpdatedUtc, 
            ImportedBy = nhOpImportedFile.ImportedBy,
            LastActionedUtc = nhOpImportedFile.LastActionedUtc,
            IsActivated = true,
            IsDeleted = false
          };
          _members.Add(importedFileProject);

          // todo split inserts and updates
          importedFileProject.ImportedFileUid = Guid.NewGuid().ToString();
          importedFileProject.FileCreatedUtc =  DateTime.MinValue; 
          importedFileProject.FileUpdatedUtc = DateTime.MinValue; 
          importedFileProject.ImportedBy = "";
          _insertMembers.Add(importedFileProject);
        }
      }
      return _members.Count;
    }

    /// <summary>
    ///  WriteToDb(insert/update) _members to NGen Project.ImportedFile table
    /// Update includes potentially setting IsDeleted flag
    /// </summary>
    public int WriteToDb()
    {
      // todo async
      // todo Upsert
      MySqlConnection dbConnection = new MySqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.WriteToDb: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.WriteToDb: open DB exeception {ex.Message}");
        throw;
      }

      // can't do upsert as importedFileId needs to be managed by DB
      // todo Merge() to split _members into 2 lists: _insertMembers list and _toUpdateMembers
      var insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted, IsActivated, LastActionedUTC) " +
        "  VALUES " +
        "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc, 0, 1, @LastActionedUtc)");
      int countInserted = 0;
      try
      {
        countInserted = dbConnection.Execute(insertCommand, _insertMembers);
        Console.WriteLine($"ImportedFileHandlerProject.WriteToDb: selectCommand {insertCommand} countInserted {JsonConvert.SerializeObject(countInserted)}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerProject.WriteToDb:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerProject.WriteToDb:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerProject.WriteToDb:  dbConnection.Close");
      }

      return countInserted;
    }
  }
}

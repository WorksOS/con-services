using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data.SqlClient;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Scheduler.Common.Interfaces;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileHandlerNhOp<T> : IImportedFileHandler<T>
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private List<NhOpImportedFile> _members = null;
    private List<NhOpImportedFile> _insertMembers = null;
    private List<NhOpImportedFile> _updateMembers = null;
    private string _dbConnectionString;

    public ImportedFileHandlerNhOp(IConfigurationStore configStore, ILoggerFactory logger,
      string dbConnectionString)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileHandlerNhOp<T>>();
      _members = new List<NhOpImportedFile>();
      _insertMembers = new List<NhOpImportedFile>();
      _updateMembers = new List<NhOpImportedFile>();
      _dbConnectionString = dbConnectionString;

      _log.LogDebug(
        $"ImportedFileHandlerNhOp.ImportedFileHandlerNhOp() _configStore {JsonConvert.SerializeObject(_configStore)}");
      Console.WriteLine(
        $"ImportedFileHandlerNhOp.ImportedFileHandlerNhOp() _configStore {JsonConvert.SerializeObject(_configStore)}");
    }

    /// <summary>
    /// ReadFromDb from NGen Project.ImportedFile table
    ///   May initially be limited to SurveyedSurface type
    /// </summary>
    public int ReadFromDb()
    {
      // todo async
      // todo use dbrepo
      SqlConnection dbConnection = new SqlConnection(_dbConnectionString);
      try
      {
        dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.ReadFromDb: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.ReadFromDb: open DB exeception {ex.Message}");
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
              iff.ID AS LegacyProjectId, CAST(p.ProjectUID AS varchar(100)) AS ProjectUid,
              c.ID AS LegacyCustomerId, CAST(c.CustomerUID AS varchar(100)) AS CustomerUid,
              fk_ImportedFileTypeID AS ImportedFileType,
              fk_DXFUnitsTypeID AS DxfUnitsType,
              iff.Name,               
              SurveyedUtc,
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
        Console.WriteLine($"ImportedFileHandlerNhOp.ReadFromDb: selectCommand {selectCommand} response {JsonConvert.SerializeObject(response)}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileHandlerNhOp.ReadFromDb:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileHandlerNhOp.ReadFromDb:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        dbConnection.Close();
        Console.WriteLine($"ImportedFileHandlerNhOp.ReadFromDb:  dbConnection.Close");
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
      throw new NotImplementedException();

      //// todo use AutoMapper
      //// todo how to match - up?
      //foreach (var nhOpImportedFile in otherSourceMembers)
      //{
      //  if (nhOpImportedFile.ImportedFileType == ImportedFileType.SurveyedSurface)
      //  {
      //    var importedFileProject = new ImportedFile
      //    {
      //      ProjectUid = nhOpImportedFile.ProjectUid,
      //      // doesn't exist in CG ImportedFileUid = ?,
      //      CustomerUid = nhOpImportedFile.CustomerUid,
      //      ImportedFileType = nhOpImportedFile.ImportedFileType,
      //      Name = nhOpImportedFile.Name,
      //      FileDescriptor = nhOpImportedFile.FileDescriptor,
      //      // doesn't exist in CG FileCreatedUtc = new DateTime(2017, 1, 1), 
      //      // doesn't exist in CG FileUpdatedUtc = new DateTime(2017, 1, 1), 
      //      // doesn't exist in CG ImportedBy = "whoever",
      //      SurveyedUtc = nhOpImportedFile.SurveyedUtc,
      //      LastActionedUtc = nhOpImportedFile.LastActionedUtc,
      //      IsActivated = true,
      //      IsDeleted = false
      //    };
      //    _members.Add(importedFileProject);

      //    // todo split inserts and updates
      //    importedFileProject.ImportedFileUid = Guid.NewGuid().ToString();
      //    importedFileProject.FileCreatedUtc =  DateTime.MinValue; 
      //    importedFileProject.FileUpdatedUtc = DateTime.MinValue; 
      //    importedFileProject.ImportedBy = "";
      //    _insertMembers.Add(importedFileProject);
      //  }
      //}
      //return _members.Count;
    }

    /// <summary>
    ///  WriteToDb(insert/update) _members to NGen Project.ImportedFile table
    /// Update includes potentially setting IsDeleted flag
    /// </summary>
    public int WriteToDb()
    {
      throw new NotImplementedException();

      //// todo async
      //// todo Upsert
      //SqlConnection dbConnection = new SqlConnection(_dbConnectionString);
      //try
      //{
      //  dbConnection.Open();
      //}
      //catch (Exception ex)
      //{
      //  _log.LogError($"ImportedFileHandlerNhOp.WriteToDb: open DB exeception {ex.Message}");
      //  Console.WriteLine($"ImportedFileHandlerNhOp.WriteToDb: open DB exeception {ex.Message}");
      //  throw;
      //}

      //// can't do upsert as importedFileId needs to be managed by DB
      //// todo Merge() to split _members into 2 lists: _insertMembers list and _toUpdateMembers
      //var insertCommand = string.Format(
      //  "INSERT ImportedFile " +
      //  "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted, IsActivated, LastActionedUTC) " +
      //  "  VALUES " +
      //  "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc, 0, 1, @LastActionedUtc)");
      //int countInserted = 0;
      //try
      //{
      //  countInserted = dbConnection.Execute(insertCommand, _insertMembers);
      //  Console.WriteLine($"ImportedFileHandlerNhOp.WriteToDb: selectCommand {insertCommand} countInserted {JsonConvert.SerializeObject(countInserted)}");
      //}
      //catch (Exception ex)
      //{
      //  _log.LogError($"ImportedFileHandlerNhOp.WriteToDb:  execute exeception {ex.Message}");
      //  Console.WriteLine($"ImportedFileHandlerNhOp.WriteToDb:  execute exeception {ex.Message}");
      //  throw;
      //}
      //finally
      //{
      //  dbConnection.Close();
      //  Console.WriteLine($"ImportedFileHandlerNhOp.WriteToDb:  dbConnection.Close");
      //}

      //return countInserted;
    }
  }
}

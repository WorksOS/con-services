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
  public class ImportedFileRepoNhOp<T> : IImportedFileRepo<T> where T : ImportedFileNhOp
  {
    private IConfigurationStore _configStore;
    private ILogger _log;
    private string _dbConnectionString;
    private SqlConnection _dbConnection;

    public ImportedFileRepoNhOp(IConfigurationStore configStore, ILoggerFactory logger)
    {
      _configStore = configStore;
      _log = logger.CreateLogger<ImportedFileRepoNhOp<T>>();
      _dbConnectionString = ConnectionUtils.GetConnectionStringMsSql(_configStore, _log, "_NH_OP");
      _dbConnection = new SqlConnection(_dbConnectionString);
    }

    public List<T> Read()
    {
      var members = new List<ImportedFileNhOp>();

      try
      {
        _dbConnection.Open(); // todo do this once-only?
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.Read: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.Read: open DB exeception {ex.Message}");
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
              iff.ID AS LegacyImportedFileId, p.ID AS LegacyProjectId, CAST(p.ProjectUID AS varchar(100)) AS ProjectUid,
              c.ID AS LegacyCustomerId, CAST(c.CustomerUID AS varchar(100)) AS CustomerUid,
              fk_ImportedFileTypeID AS ImportedFileType, iff.Name,               
              SurveyedUtc, fk_DXFUnitsTypeID AS DxfUnitsType,
              FileCreatedUtc, FileUpdatedUtc, u.EmailContact AS ImportedBy,
              iff.InsertUTC AS LastActionedUtc
            FROM ImportedFile iff
              INNER JOIN Customer c ON c.ID = fk_CustomerID
              INNER JOIN Project p ON p.ID = fk_ProjectID 
              OUTER APPLY (SELECT TOP 1 CreateUTC AS FileCreatedUtc, InsertUtc AS FileUpdatedUtc, fk_UserID FROM ImportedFileHistory WHERE fk_ImportedFileID = iff.ID ORDER BY InsertUTC desc) ifhLast
              -- OUTER APPLY (SELECT TOP 1 CreateUTC AS FileCreatedUtc FROM ImportedFileHistory WHERE fk_ImportedFileID = iff.ID ORDER BY InsertUTC asc) ifhFirst
              LEFT OUTER JOIN [User] u on u.id = ifhLast.fk_UserID
            WHERE fk_ImportedFileTypeID = 2";
      List<ImportedFileNhOp> response;
      try
      {
        response = _dbConnection.Query<ImportedFileNhOp>(selectCommand).ToList();
        _log.LogTrace($"ImportedFileRepoNhOp.Read: responseCount {response.Count}");
        Console.WriteLine($"ImportedFileRepoNhOp.Read: responseCount {response.Count}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.Read:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.Read:  execute exeception {ex.Message}");
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
      long returnedImportedFileId = 0;
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.Create: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.Create: open DB exeception {ex.Message}");
        throw;
      }

      var insertImportedFileCommand = string.Format(
        " INSERT ImportedFile " +
        "    (fk_CustomerID, fk_ProjectID, Name, fk_ImportedFileTypeID, SurveyedUTC, fk_DXFUnitsTypeID) " +
        " OUTPUT INSERTED.[Id] " +
        "  VALUES " +
        "    (@LegacyCustomerId, @LegacyProjectId, @Name, @ImportedFileType, @SurveyedUtc, @DxfUnitsType);");

      int countInserted = 0;
      try
      {
        returnedImportedFileId = _dbConnection.QuerySingle<long>(insertImportedFileCommand, member);
        if (returnedImportedFileId > 0)
          countInserted++;
        _log.LogTrace(
          $"ImportedFileRepoNhOp.Create: member {JsonConvert.SerializeObject(member)} countInserted {countInserted}");
        Console.WriteLine(
          $"ImportedFileRepoNhOp.Create: member {JsonConvert.SerializeObject(member)} countInserted {countInserted}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.Create:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.Create:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }
      if (returnedImportedFileId > 0)
      {
        member.LegacyImportedFileId = returnedImportedFileId;
        CreateHistory(member);
      }
      return returnedImportedFileId;
    }

    public int CreateHistory(T member)
    {
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.CreateHistory: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.CreateHistory: open DB exeception {ex.Message}");
        throw;
      }

      // todo don't know the legacy UserID
      var insertImportedFileHistoryeCommand = string.Format(
        "INSERT ImportedFileHistory " +
        "    (fk_ImportedFileID, InsertUTC, CreateUTC, fk_UserID) " +
        "  VALUES " +
        "    (@LegacyImportedFileId, @FileUpdatedUtc, @FileCreatedUtc, 0)");

      int countInserted = 0;
      try
      {
        countInserted += _dbConnection.Execute(insertImportedFileHistoryeCommand,
          new {member.FileCreatedUtc, member.FileUpdatedUtc, member.LegacyImportedFileId});
        _log.LogTrace($"ImportedFileRepoNhOp.CreateHistory: countInserted {countInserted}");
        Console.WriteLine($"ImportedFileRepoNhOp.CreateHistory: countInserted {countInserted}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.CreateHistory:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.CreateHistory:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }
      return countInserted;
    }

    /// <summary>
    ///  the only thing 'updateable' in a CG ImportFile is to add another history row
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public int Update(T member)
    {
      return CreateHistory(member);
    }

    public int Delete(T member)
    {
      try
      {
        _dbConnection.Open();
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.Delete: open DB exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.Delete: open DB exeception {ex.Message}");
        throw;
      }

      var deleteImportedFileCommand =
        @"DELETE ImportedFile                
                WHERE ID = @LegacyImportedFileId";

      var deleteImportedFileHistoryCommand =
        @"DELETE ImportedFileHistory                
                WHERE fk_ImportedFileID = @LegacyImportedFileId";

      int countUpdated = 0;
      try
      {
        countUpdated += _dbConnection.Execute(deleteImportedFileCommand, member);
        _log.LogTrace($"ImportedFileRepoNhOp.Delete(ImportedFile): countUpdatedSoFar {countUpdated}");
        Console.WriteLine($"ImportedFileRepoNhOp.Delete(ImportedFile): countUpdatedSoFar {countUpdated}");

        if (countUpdated > 0)
          countUpdated += _dbConnection.Execute(deleteImportedFileHistoryCommand, member);
        _log.LogTrace($"ImportedFileRepoNhOp.Delete(ImportedFileHistory): countUpdatedFinal {countUpdated}");
        Console.WriteLine($"ImportedFileRepoNhOp.Delete(ImportedFileHistory): countUpdatedFinal {countUpdated}");
      }
      catch (Exception ex)
      {
        _log.LogError($"ImportedFileRepoNhOp.Delete:  execute exeception {ex.Message}");
        Console.WriteLine($"ImportedFileRepoNhOp.Delete:  execute exeception {ex.Message}");
        throw;
      }
      finally
      {
        _dbConnection.Close();
      }

      return countUpdated;
    }
  }
}
